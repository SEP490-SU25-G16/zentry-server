// File: Zentry.Modules.ScheduleManagement.Application.Features.GetAllClassSectionsWithEnrollmentCount/GetAllClassSectionsWithEnrollmentCountQueryHandler.cs

using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Schedule;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetAllClassSectionsWithEnrollmentCount;

public class GetAllClassSectionsWithEnrollmentCountQueryHandler(
    IClassSectionRepository classSectionRepository,
    IEnrollmentRepository enrollmentRepository,
    IMediator mediator
) : IQueryHandler<GetAllClassSectionsWithEnrollmentCountQuery, List<ClassSectionWithEnrollmentCountDto>>
{
    public async Task<List<ClassSectionWithEnrollmentCountDto>> Handle(
        GetAllClassSectionsWithEnrollmentCountQuery request,
        CancellationToken cancellationToken)
    {
        // Lấy tất cả ClassSection và đảm bảo include Course và Enrollment
        var classSections = await classSectionRepository.GetAllAsync(cancellationToken);

        // Lọc ra các ClassSection đã xóa
        var activeClassSections = classSections.Where(cs => !cs.IsDeleted).ToList();

        if (!activeClassSections.Any())
        {
            return new List<ClassSectionWithEnrollmentCountDto>();
        }

        // 1. Thu thập tất cả LecturerId cần tra cứu
        var lecturerIds = activeClassSections
            .Where(cs => cs.LecturerId.HasValue)
            .Select(cs => cs.LecturerId!.Value)
            .Distinct()
            .ToList();

        // 2. Tra cứu tất cả giảng viên cùng một lúc bằng IMediator
        var lecturers = new Dictionary<Guid, BasicUserInfoDto>();
        if (lecturerIds.Any())
        {
            var lecturerLookupResponse =
                await mediator.Send(new GetUsersByIdsIntegrationQuery(lecturerIds), cancellationToken);
            lecturers = lecturerLookupResponse.Users.ToDictionary(u => u.Id, u => u);
        }

        // 3. Ánh xạ DTO một cách hiệu quả
        var result = activeClassSections.Select(cs =>
        {
            BasicUserInfoDto? lecturerInfo = null;
            if (cs.LecturerId.HasValue)
            {
                lecturers.TryGetValue(cs.LecturerId.Value, out lecturerInfo);
            }

            return new ClassSectionWithEnrollmentCountDto
            {
                Id = cs.Id,
                CourseId = cs.CourseId,
                CourseCode = cs.Course?.Code ?? "N/A",
                CourseName = cs.Course?.Name ?? "N/A",
                LecturerId = cs.LecturerId,
                LecturerName = lecturerInfo?.FullName ?? "Unassigned Lecturer",
                SectionCode = cs.SectionCode,
                Semester = cs.Semester,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                EnrolledStudentsCount =
                    cs.Enrollments?.Count(e => e.Status == EnrollmentStatus.Active) ??
                    0 // Đếm sinh viên có status "Active"
            };
        }).OrderBy(dto => dto.SectionCode).ToList();

        return result;
    }
}
