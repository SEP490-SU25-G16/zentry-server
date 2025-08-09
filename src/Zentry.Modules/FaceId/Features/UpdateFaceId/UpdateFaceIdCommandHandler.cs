using MediatR;
using Pgvector;
using Zentry.Modules.FaceId.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.FaceId.Features.UpdateFaceId;

public class UpdateFaceIdCommandHandler : ICommandHandler<UpdateFaceIdCommand, UpdateFaceIdResponse>
{
    private readonly IFaceIdRepository _faceIdRepository;
    private readonly IMediator _mediator;

    public UpdateFaceIdCommandHandler(IFaceIdRepository faceIdRepository, IMediator mediator)
    {
        _faceIdRepository = faceIdRepository;
        _mediator = mediator;
    }

    public async Task<UpdateFaceIdResponse> Handle(UpdateFaceIdCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user has a face ID
            var exists = await _faceIdRepository.ExistsByUserIdAsync(command.UserId, cancellationToken);
            if (!exists)
                return new UpdateFaceIdResponse
                {
                    Success = false,
                    Message = "User does not have a registered Face ID. Use register instead."
                };

            // Convert float array to Vector
            var embedding = new Vector(command.EmbeddingArray);

            // Update embedding in database
            await _faceIdRepository.UpdateAsync(command.UserId, embedding, cancellationToken);

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