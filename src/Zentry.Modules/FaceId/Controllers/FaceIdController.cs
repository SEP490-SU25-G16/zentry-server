using System.IO;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.FaceId.Features.RegisterFaceId;
using Zentry.Modules.FaceId.Features.UpdateFaceId;
using Zentry.Modules.FaceId.Features.VerifyFaceId;

namespace Zentry.Modules.FaceId.Controllers;

[ApiController]
[Route("api/faceid")]
public class FaceIdController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterFaceIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromForm] string userId, IFormFile embedding)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            if (embedding == null)
                return BadRequest("Embedding file is required");

            // Read embedding bytes
            using var memoryStream = new MemoryStream();
            await embedding.CopyToAsync(memoryStream);
            byte[] embeddingBytes = memoryStream.ToArray();

            // Convert bytes to float array (4 bytes per float)
            float[] embeddingArray = new float[embeddingBytes.Length / 4];
            Buffer.BlockCopy(embeddingBytes, 0, embeddingArray, 0, embeddingBytes.Length);

            // Create and send command
            var command = new RegisterFaceIdCommand(Guid.Parse(userId), embeddingArray);
            var result = await mediator.Send(command);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error: " + ex.Message,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }

    [HttpPost("update")]
    [ProducesResponseType(typeof(UpdateFaceIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromForm] string userId, IFormFile embedding)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            if (embedding == null)
                return BadRequest("Embedding file is required");

            // Read embedding bytes
            using var memoryStream = new MemoryStream();
            await embedding.CopyToAsync(memoryStream);
            byte[] embeddingBytes = memoryStream.ToArray();

            // Convert bytes to float array (4 bytes per float)
            float[] embeddingArray = new float[embeddingBytes.Length / 4];
            Buffer.BlockCopy(embeddingBytes, 0, embeddingArray, 0, embeddingBytes.Length);

            // Create and send command
            var command = new UpdateFaceIdCommand(Guid.Parse(userId), embeddingArray);
            var result = await mediator.Send(command);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error: " + ex.Message,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyFaceIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Verify([FromForm] string userId, IFormFile embedding, [FromForm] float? threshold = null)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            if (embedding == null)
                return BadRequest("Embedding file is required");

            // Read embedding bytes
            using var memoryStream = new MemoryStream();
            await embedding.CopyToAsync(memoryStream);
            byte[] embeddingBytes = memoryStream.ToArray();

            // Convert bytes to float array (4 bytes per float)
            float[] embeddingArray = new float[embeddingBytes.Length / 4];
            Buffer.BlockCopy(embeddingBytes, 0, embeddingArray, 0, embeddingBytes.Length);

            // Create and send command
            var command = new VerifyFaceIdCommand(
                Guid.Parse(userId), 
                embeddingArray, 
                threshold ?? 0.7f);
                
            var result = await mediator.Send(command);

            // Always return 200 OK, with Success = true/false in body
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error: " + ex.Message,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
        }
    }
} 
