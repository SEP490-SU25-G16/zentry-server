﻿using System.Collections.Generic;
using System.Linq;
using Zentry.Modules.FaceId.Entities;
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
        // Fetch all verify requests for the session and provided students
        var verifyRequests = await faceIdRepository.GetVerifyRequestsBySessionAndUsersAsync(
            request.SessionId,
            request.StudentIds,
            cancellationToken);

        // Group by student and compute matched according to business rules:
        // - If any request for a student is Completed with Matched == true => matched = true
        // - Else if any request is Completed with Matched == false => matched = false
        // - Else if any request is Expired or Canceled or Pending => matched = false by default
        var statusMap = new Dictionary<Guid, StudentFaceId>();

        var requestsByUser = verifyRequests.GroupBy(r => r.TargetUserId);
        foreach (var group in requestsByUser)
        {
            var userId = group.Key;
            var anyCompletedFalse = group.Any(r => r.Status == FaceIdVerifyRequestStatus.Completed && r.Matched == false);
            var anyCompletedTrue = group.Any(r => r.Status == FaceIdVerifyRequestStatus.Completed && r.Matched == true);

            bool matched;
            // Business rule: any failure dominates => overall false
            if (anyCompletedFalse)
            {
                matched = false;
            }
            else if (anyCompletedTrue)
            {
                matched = true;
            }
            else
            {
                // All are non-completed (Pending/Expired/Canceled) or no records for user => default false
                matched = false;
            }

            statusMap[userId] = new StudentFaceId(userId, matched);
        }

        // Ensure all requested students appear in result, even if no verify request rows exist
        foreach (var sid in request.StudentIds)
        {
            if (!statusMap.ContainsKey(sid))
            {
                statusMap[sid] = new StudentFaceId(sid, false);
            }
        }

        return new GetFaceIdResultByStudentIdsAndSessionIdIntegrationResponse(statusMap);
    }
}