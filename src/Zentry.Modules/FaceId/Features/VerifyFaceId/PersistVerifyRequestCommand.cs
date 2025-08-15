using Zentry.Modules.FaceId.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.FaceId.Features.VerifyFaceId;

public record PersistVerifyRequestCommand(
    Guid RequestGroupId,
    Guid TargetUserId,
    Guid? InitiatorUserId,
    Guid? SessionId,
    Guid? ClassSectionId,
    float Threshold,
    DateTime ExpiresAt) : ICommand<bool>;

public class PersistVerifyRequestCommandHandler(IFaceIdRepository repository)
    : ICommandHandler<PersistVerifyRequestCommand, bool>
{
    public async Task<bool> Handle(PersistVerifyRequestCommand command, CancellationToken cancellationToken)
    {
        await repository.CreateVerifyRequestAsync(
            command.RequestGroupId,
            command.TargetUserId,
            command.InitiatorUserId,
            command.SessionId,
            command.ClassSectionId,
            command.Threshold,
            command.ExpiresAt,
            cancellationToken);
        return true;
    }
}

public record CompleteVerifyRequestCommand(
    Guid TargetUserId,
    Guid? SessionId,
    Guid RequestId,
    bool Matched,
    float Similarity,
    bool completeIfFailed = false) : ICommand<bool>;

public class CompleteVerifyRequestCommandHandler(IFaceIdRepository repository)
    : ICommandHandler<CompleteVerifyRequestCommand, bool>
{
    public async Task<bool> Handle(CompleteVerifyRequestCommand command, CancellationToken cancellationToken)
    {
        // This is a simplified completion; a real impl would locate exact request entity by id
        var req = await repository.GetVerifyRequestAsync(command.RequestId, cancellationToken);
        if (req is null) return false;

        if (command.Matched || command.completeIfFailed)
        {
            await repository.CompleteVerifyRequestAsync(req, command.Matched, command.Similarity, cancellationToken);
        }
        return true;
    }
}


