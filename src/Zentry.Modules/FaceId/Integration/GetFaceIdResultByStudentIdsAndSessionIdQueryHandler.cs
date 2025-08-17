using Zentry.Modules.FaceId.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.FaceId;

namespace Zentry.Modules.FaceId.Integration;

public class GetFaceIdResultByStudentIdsAndSessionIdQueryHandler(IFaceIdRepository faceIdRepository)
    : IQueryHandler<GetFaceIdResultByStudentIdsAndSessionIdIntegrationQuery,
        GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse>
{
    public async Task<GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse> Handle(
        GetFaceIdResultByStudentIdsAndSessionIdIntegrationQuery request, CancellationToken cancellationToken)
    {
        var studentStatus = new Dictionary<Guid, StudentFaceId>();
        return new GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse(studentStatus);
    }
}