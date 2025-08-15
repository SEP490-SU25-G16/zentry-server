using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.FaceId.Features.VerifyFaceId;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.Modules.FaceId.Interfaces;

namespace Zentry.Modules.FaceId.Controllers;

[ApiController]
[Route("api/faceid/requests")]
public class FaceVerificationRequestsController : ControllerBase
{
    private readonly ILogger<FaceVerificationRequestsController> _logger;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRedisService _redis;
    private readonly IFaceIdRepository _repository;

    public FaceVerificationRequestsController(
        ILogger<FaceVerificationRequestsController> logger,
        IMediator mediator,
        IPublishEndpoint publishEndpoint,
        IRedisService redis,
        IFaceIdRepository repository)
    {
        _logger = logger;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _redis = redis;
        _repository = repository;
    }

    public class CreateFaceVerificationRequestDto
    {
        public Guid LecturerId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? ClassSectionId { get; set; }
        public List<Guid>? RecipientUserIds { get; set; }
        public int? ExpiresInMinutes { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
    }

    public class CreateFaceVerificationResponseDto
    {
        public required Guid RequestId { get; init; }
        public required Guid SessionId { get; init; }
        public required DateTime ExpiresAt { get; init; }
        public required int TotalRecipients { get; init; }
        public required float Threshold { get; init; }
    }

    private record FaceVerificationRequestMeta(
        Guid RequestId,
        Guid SessionId,
        Guid LecturerId,
        Guid? ClassSectionId,
        DateTime ExpiresAt,
        string? Title,
        string? Body,
        List<Guid> Recipients
    );

    private record FaceVerificationReceipt(
        Guid RequestId,
        Guid UserId,
        bool Success,
        float Similarity,
        DateTime VerifiedAt
    );

    [HttpPost]
    [ProducesResponseType(typeof(CreateFaceVerificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFaceVerificationRequestDto request, CancellationToken cancellationToken)
    {
        if (request.LecturerId == Guid.Empty)
            return BadRequest(new { Message = "LecturerId is required" });

        if (request.SessionId == Guid.Empty)
            return BadRequest(new { Message = "SessionId is required" });

        if ((request.ClassSectionId is null || request.ClassSectionId == Guid.Empty) && (request.RecipientUserIds is null || request.RecipientUserIds.Count == 0))
            return BadRequest(new { Message = "Provide either ClassSectionId or RecipientUserIds" });

        try
        {
            // 1) Resolve recipients
            List<Guid> recipients;
            if (request.RecipientUserIds is not null && request.RecipientUserIds.Count > 0)
            {
                recipients = request.RecipientUserIds.Distinct().ToList();
            }
            else
            {
                var resp = await _mediator.Send(new GetStudentIdsByClassSectionIdIntegrationQuery(request.ClassSectionId!.Value), cancellationToken);
                recipients = resp.StudentIds.Distinct().ToList();
            }

            if (recipients.Count == 0)
                return BadRequest(new { Message = "No recipients found" });

            // 2) Build request meta and store to redis
            var requestId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var expiresInMinutes = Math.Max(1, request.ExpiresInMinutes.GetValueOrDefault(30));
            var expiresAt = now.AddMinutes(expiresInMinutes);
            var threshold = 0.7f;

            var meta = new FaceVerificationRequestMeta(
                requestId,
                request.SessionId,
                request.LecturerId,
                request.ClassSectionId,
                expiresAt,
                request.Title,
                request.Body,
                recipients);

            var metaKey = $"faceid:req:{requestId}:meta";
            await _redis.SetAsync(metaKey, meta, expiresAt - now);

            // Persist requests (one per recipient) to DB for auditing/expiry
            foreach (var uid in recipients)
            {
                await _mediator.Send(new PersistVerifyRequestCommand(requestId, uid, request.LecturerId, request.SessionId, request.ClassSectionId, threshold, expiresAt), cancellationToken);
            }

            // 3) Push notifications
            var title = string.IsNullOrWhiteSpace(request.Title) ? "Yêu cầu xác thực Face ID" : request.Title!;
            var body = string.IsNullOrWhiteSpace(request.Body) ? "Vui lòng xác thực khuôn mặt để tiếp tục." : request.Body!;
            var deeplink = $"zentry://face-verify?requestId={requestId}&sessionId={request.SessionId}";

            var publishTasks = recipients.Select(userId => _publishEndpoint.Publish(new NotificationCreatedEvent
            {
                Title = title,
                Body = body,
                RecipientUserId = userId,
                Type = NotificationType.All,
                Data = new Dictionary<string, string>
                {
                    ["type"] = "FACE_VERIFICATION_REQUEST",
                    ["requestId"] = requestId.ToString(),
                    ["sessionId"] = request.SessionId.ToString(),
                    ["deeplink"] = deeplink,
                    ["action"] = "VERIFY_FACE_ID"
                }
            }, cancellationToken));

            await Task.WhenAll(publishTasks);

            // 4) Return
            var response = new CreateFaceVerificationResponseDto
            {
                RequestId = requestId,
                SessionId = request.SessionId,
                ExpiresAt = expiresAt,
                TotalRecipients = recipients.Count,
                Threshold = threshold
            };

            return CreatedAtAction(nameof(Create), new { id = requestId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create face verification request for session {SessionId}", request.SessionId);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("{requestId:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(
        Guid requestId,
        [FromForm] string userId,
        IFormFile embedding,
        [FromForm] float? threshold,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { Message = "userId is required" });
        if (embedding is null)
            return BadRequest(new { Message = "embedding file is required" });

        var metaKey = $"faceid:req:{requestId}:meta";
        var meta = await _redis.GetAsync<FaceVerificationRequestMeta>(metaKey);
        if (meta is null)
            return NotFound(new { Message = "Request not found or expired" });
        if (DateTime.UtcNow > meta.ExpiresAt)
            return StatusCode(StatusCodes.Status410Gone, new { Message = "Request expired" });

        var parsedUserId = Guid.Parse(userId);
        if (!meta.Recipients.Contains(parsedUserId))
            return BadRequest(new { Message = "User is not a recipient of this request" });

        // read embedding bytes → float array
        using var memoryStream = new MemoryStream();
        await embedding.CopyToAsync(memoryStream, cancellationToken);
        var embeddingBytes = memoryStream.ToArray();
        var embeddingArray = new float[embeddingBytes.Length / 4];
        Buffer.BlockCopy(embeddingBytes, 0, embeddingArray, 0, embeddingBytes.Length);

        // verify via FaceId module handler
        var cmd = new VerifyFaceIdCommand(parsedUserId, embeddingArray, threshold ?? 0.7f, requestId);
        var result = await _mediator.Send(cmd, cancellationToken);

        // store receipt regardless of success for auditing
        var ttl = meta.ExpiresAt - DateTime.UtcNow;
        if (ttl < TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);

        var receipt = new FaceVerificationReceipt(
            requestId,
            parsedUserId,
            result.Success,
            result.Similarity,
            DateTime.UtcNow);

        var userKey = $"faceid:req:{requestId}:user:{parsedUserId}";
        await _redis.SetAsync(userKey, receipt, ttl);

        if (result.Success)
        {
            // update verified list
            var verifiedKey = $"faceid:req:{requestId}:verified";
            var current = await _redis.GetAsync<List<Guid>>(verifiedKey) ?? new List<Guid>();
            if (!current.Contains(parsedUserId))
            {
                current.Add(parsedUserId);
                await _redis.SetAsync(verifiedKey, current, ttl);
            }

            // Mark DB request completed for this user
            await _mediator.Send(new CompleteVerifyRequestCommand(parsedUserId, meta.SessionId, requestId, true, result.Similarity), cancellationToken);
        }
        else
        {
            // Record failed attempt
            await _mediator.Send(new CompleteVerifyRequestCommand(parsedUserId, meta.SessionId, requestId, false, result.Similarity, completeIfFailed: true), cancellationToken);
        }

        return Ok(new
        {
            Success = result.Success,
            Similarity = result.Similarity,
            VerifiedAt = receipt.VerifiedAt
        });
    }

    public class FaceVerificationStatusResponse
    {
        public required Guid RequestId { get; init; }
        public required Guid SessionId { get; init; }
        public required DateTime ExpiresAt { get; init; }
        public required int TotalRecipients { get; init; }
        public required int TotalVerified { get; init; }
        public required List<Guid> VerifiedUserIds { get; init; }
    }

    [HttpGet("{requestId:guid}/status")]
    [ProducesResponseType(typeof(FaceVerificationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid requestId)
    {
        var metaKey = $"faceid:req:{requestId}:meta";
        var meta = await _redis.GetAsync<FaceVerificationRequestMeta>(metaKey);
        if (meta is null)
            return NotFound(new { Message = "Request not found or expired" });

        var verifiedKey = $"faceid:req:{requestId}:verified";
        var verified = await _redis.GetAsync<List<Guid>>(verifiedKey) ?? new List<Guid>();

        var response = new FaceVerificationStatusResponse
        {
            RequestId = meta.RequestId,
            SessionId = meta.SessionId,
            ExpiresAt = meta.ExpiresAt,
            TotalRecipients = meta.Recipients.Count,
            TotalVerified = verified.Count,
            VerifiedUserIds = verified
        };

        return Ok(response);
    }

    [HttpPatch("{requestId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid requestId, CancellationToken cancellationToken)
    {
        var metaKey = $"faceid:req:{requestId}:meta";
        var meta = await _redis.GetAsync<FaceVerificationRequestMeta>(metaKey);
        if (meta is null)
            return NotFound(new { Message = "Request not found or expired" });

        // Notify recipients that the verification session ended early
        var title = "Phiên xác thực kết thúc";
        var body = "Giảng viên đã kết thúc phiên học sớm. Bạn không cần xác thực nữa.";
        var publishTasks = meta.Recipients.Select(userId => _publishEndpoint.Publish(new NotificationCreatedEvent
        {
            Title = title,
            Body = body,
            RecipientUserId = userId,
            Type = NotificationType.All,
            Data = new Dictionary<string, string>
            {
                ["type"] = "FACE_VERIFICATION_CANCELED",
                ["requestId"] = requestId.ToString(),
                ["sessionId"] = meta.SessionId.ToString(),
                ["action"] = "CLOSE_VERIFY"
            }
        }, cancellationToken));
        await Task.WhenAll(publishTasks);

        await _repository.CancelVerifyRequestsByGroupAsync(requestId, cancellationToken);

        // Also remove redis keys so clients stop seeing it
        await _redis.RemoveAsync(metaKey);
        await _redis.RemoveAsync($"faceid:req:{requestId}:verified");

        return NoContent();
    }
}


