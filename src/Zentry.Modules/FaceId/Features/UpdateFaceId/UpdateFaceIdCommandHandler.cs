using MediatR;
using Microsoft.Extensions.Options;
using Zentry.Modules.FaceId.Configuration;
using Zentry.Modules.FaceId.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.FaceId.Features.UpdateFaceId;

public class UpdateFaceIdCommandHandler : ICommandHandler<UpdateFaceIdCommand, UpdateFaceIdResponse>
{
    private readonly IFaceIdRepository _faceIdRepository;
    private readonly IMediator _mediator;
    private readonly IOptions<FaceIdSettings> _faceIdSettings;

    public UpdateFaceIdCommandHandler(IFaceIdRepository faceIdRepository, IMediator mediator, IOptions<FaceIdSettings> faceIdSettings)
    {
        _faceIdRepository = faceIdRepository;
        _mediator = mediator;
        _faceIdSettings = faceIdSettings;
    }

    public async Task<UpdateFaceIdResponse> Handle(UpdateFaceIdCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var updateThreshold = _faceIdSettings.Value.UpdateThreshold;
            // Check if user has a face ID
            var exists = await _faceIdRepository.ExistsByUserIdAsync(command.UserId, cancellationToken);
            if (!exists)
                return new UpdateFaceIdResponse
                {
                    Success = false,
                    Message = "User does not have a registered Face ID. Use register instead."
                };

            // Verify similarity with existing embedding before allowing update (now accepts float[] directly)
            var (isMatch, similarity) = await _faceIdRepository.VerifyAsync(
                command.UserId,
                command.EmbeddingArray,
                updateThreshold,
                cancellationToken);

            if (!isMatch)
                return new UpdateFaceIdResponse
                {
                    Success = false,
                    Message =
                        $"Face ID update rejected: similarity {similarity:F3} below threshold {updateThreshold}"
                };

            // Update embedding in database (now accepts float[] directly)
            await _faceIdRepository.UpdateAsync(command.UserId, command.EmbeddingArray, cancellationToken);

            // Update user's face ID status (to update the LastUpdated timestamp)
            var updateFaceIdCommand =
                new UserManagement.Features.UpdateFaceId.UpdateFaceIdCommand(command.UserId, true);
            await _mediator.Send(updateFaceIdCommand, cancellationToken);

            return new UpdateFaceIdResponse
            {
                Success = true,
                Message = "Face ID updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new UpdateFaceIdResponse
            {
                Success = false,
                Message = $"Error updating Face ID: {ex.Message}"
            };
        }
    }
}