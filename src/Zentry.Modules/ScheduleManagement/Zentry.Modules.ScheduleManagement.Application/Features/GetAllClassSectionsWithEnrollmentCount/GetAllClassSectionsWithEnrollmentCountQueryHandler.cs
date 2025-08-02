using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

// Để lấy thông tin User/Lecturer Name
// For EnrollmentStatus

// For Role

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetAllClassSectionsWithEnrollmentCount;

public class GetAllClassSectionsWithEnrollmentCountQueryHandler(
    IClassSectionRepository classSectionRepository,
    IEnrollmentRepository enrollmentRepository, // Cần để đếm số sinh viên
    IUserScheduleService userScheduleService // Để lấy tên giảng viên
) : IQueryHandler<GetAllClassSectionsWithEnrollmentCountQuery, List<ClassSectionWithEnrollmentCountDto>>
{
    public async Task<List<ClassSectionWithEnrollmentCountDto>> Handle(
        GetAllClassSectionsWithEnrollmentCountQuery request,
        CancellationToken cancellationToken)
    {
        // Lấy tất cả các ClassSection
        // Cần đảm bảo ClassSectionRepository.GetAllAsync() include Course và Enrollments (hoặc là nó sẽ join sau)
        // Hiện tại GetAllAsync() của bạn chỉ Include(cs => cs.Course), cần thêm Include(cs => cs.Enrollments)
        var classSections = await classSectionRepository.GetAllAsync(cancellationToken);

        var result = new List<ClassSectionWithEnrollmentCountDto>();

        foreach (var cs in classSections)
        {
            // Bỏ qua các class section đã bị xóa mềm
            if (cs.IsDeleted) continue;

            // Lấy tổng số sinh viên đang hoạt động trong ClassSection này
            // Sử dụng EnrollmentRepository để lấy số lượng sinh viên đang hoạt động
            var enrolledStudentsCount = await enrollmentRepository.GetActiveStudentIdsByClassSectionIdAsync(
                cs.Id, cancellationToken);

            // Lấy thông tin giảng viên (nếu cần)
            var lecturer =
                await userScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, cs.LecturerId, cancellationToken);
            var lecturerName = lecturer?.FullName ?? "N/A";


            result.Add(new ClassSectionWithEnrollmentCountDto
            {
                Id = cs.Id,
                CourseId = cs.CourseId,
                CourseCode = cs.Course?.Code ?? "N/A", // Đảm bảo Course không null
                CourseName = cs.Course?.Name ?? "N/A", // Đảm bảo Course không null
                LecturerId = cs.LecturerId,
                LecturerName = lecturerName,
                SectionCode = cs.SectionCode,
                Semester = cs.Semester,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                EnrolledStudentsCount = enrolledStudentsCount.Count // Số lượng sinh viên
            });
        }

        return result.OrderBy(dto => dto.SectionCode).ToList();
    }
}