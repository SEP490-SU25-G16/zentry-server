using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.FaceId;

public record
    GetFaceIdResultByStudentIdsAndSessionIdIntegrationQuery
    : IQuery<GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse>;

public record GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse(Dictionary<Guid, StudentFaceId> studentStatus);

public record StudentFaceId(Guid StudentId, Guid SesssionId, bool Matched);