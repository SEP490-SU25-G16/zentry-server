using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

public class GetEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    ICourseRepository courseRepository,
    IUserScheduleService userScheduleService)
    : ICommandHandler<GetEnrollmentsQuery, GetEnrollmentsResponse>
{
    public async Task<GetEnrollmentsResponse> Handle(GetEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra CourseId nếu cần
        if (query.CourseId.HasValue && query.CourseId.Value != Guid.Empty)
        {
            var courseExists = await courseRepository.GetByIdAsync(query.CourseId.Value, cancellationToken);
            if (courseExists == null)
                throw new NotFoundException("Course", query.CourseId.Value);
        }

        // 2. Tạo tiêu chí tìm kiếm
        var criteria = new EnrollmentListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            StudentId = query.StudentId,
            ClassSectionId = query.ClassSectionId,
            CourseId = query.CourseId,
            Status = query.Status,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };

        // 3. Lấy danh sách Enrollment
        var (enrollments, totalCount) =
            await enrollmentRepository.GetPagedEnrollmentsAsync(criteria, cancellationToken);

        var enrollmentItems = new List<EnrollmentDto>();

        // 4. Lookup student
        var studentIds = enrollments.Select(e => e.StudentId).Distinct();
        var students = new Dictionary<Guid, GetUserByIdAndRoleIntegrationResponse>();
        foreach (var studentId in studentIds)
        {
            var studentDto =
                await userScheduleService.GetUserByIdAndRoleAsync("student", studentId, cancellationToken);
            if (studentDto != null)
                students[studentId] = studentDto;
        }

        // 5. Lookup lecturer
        var lecturerIds = enrollments.Select(e => e.ClassSection.LecturerId).Distinct();
        var lecturers = new Dictionary<Guid, GetUserByIdAndRoleIntegrationResponse>();
        foreach (var lecturerId in lecturerIds)
        {
            var lecturerDto =
                await userScheduleService.GetUserByIdAndRoleAsync("Lecturer", lecturerId, cancellationToken);
            if (lecturerDto != null)
                lecturers[lecturerId] = lecturerDto;
        }

        // 6. Ánh xạ DTO
        foreach (var enrollment in enrollments)
        {
            students.TryGetValue(enrollment.StudentId, out var studentDto);
            lecturers.TryGetValue(enrollment.ClassSection.LecturerId, out var lecturerDto);

            enrollmentItems.Add(new EnrollmentDto
            {
                EnrollmentId = enrollment.Id,
                EnrollmentDate = enrollment.EnrolledAt,
                StudentId = enrollment.StudentId,
                StudentName = studentDto?.FullName,
                ClassSectionId = enrollment.ClassSectionId,
                ClassSectionCode = enrollment.ClassSection?.SectionCode,
                CourseId = enrollment.ClassSection.CourseId,
                CourseCode = enrollment.ClassSection.Course?.Code,
                CourseName = enrollment.ClassSection.Course?.Name,
                LecturerId = enrollment.ClassSection.LecturerId,
                LecturerName = lecturerDto?.FullName,
                Status = enrollment.Status.ToString()
            });
        }

        return new GetEnrollmentsResponse
        {
            Items = enrollmentItems,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}