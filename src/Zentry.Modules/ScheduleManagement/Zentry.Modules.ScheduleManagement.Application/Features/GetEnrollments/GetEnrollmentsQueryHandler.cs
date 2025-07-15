using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

public class GetEnrollmentsQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IScheduleRepository scheduleRepository,
    ICourseRepository courseRepository, // Inject ICourseRepository
    IUserScheduleService userScheduleService)
    : ICommandHandler<GetEnrollmentsQuery, GetEnrollmentsResponse>
{
    private readonly ICourseRepository
        _courseRepository = courseRepository; // Mới: để kiểm tra CourseId tồn tại và lấy Course Name/Code

    private readonly IEnrollmentRepository _enrollmentRepository = enrollmentRepository;

    private readonly IScheduleRepository
        _scheduleRepository = scheduleRepository; // Để kiểm tra CourseId (nếu cần), và lấy chi tiết Schedule

    private readonly IUserScheduleService
        _userScheduleService = userScheduleService; // Sử dụng UserScheduleService thay vì UserLookupService trực tiếp

    // Inject IUserScheduleService

    public async Task<GetEnrollmentsResponse> Handle(GetEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        // --- 2. Kiểm tra CourseId tồn tại nếu được cung cấp ---
        if (query.CourseId.HasValue && query.CourseId.Value != Guid.Empty)
        {
            var courseExists = await _courseRepository.GetByIdAsync(query.CourseId.Value, cancellationToken);
            if (courseExists == null) throw new NotFoundException("Course", query.CourseId.Value); // Lỗi 404
        }

        // --- 3. Tạo tiêu chí tìm kiếm ---
        var criteria = new EnrollmentListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            StudentId = query.StudentId,
            ScheduleId = query.ScheduleId,
            CourseId = query.CourseId,
            Status = query.Status,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };

        // --- 4. Lấy danh sách ghi danh có phân trang ---
        // EnrollmentRepository sẽ bao gồm (Include) Schedule, Course, Room, Lecturer
        var (enrollments, totalCount) =
            await _enrollmentRepository.GetPagedEnrollmentsAsync(criteria, cancellationToken);

        // --- 5. Lấy thông tin bổ sung cho DTO và ánh xạ ---
        var enrollmentItems = new List<EnrollmentListItemDto>();

        // Thu thập tất cả StudentIds cần lookup
        var studentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();

        // Thực hiện lookup các thông tin User một lần (nếu UserLookupService hỗ trợ batch lookup, thì tốt hơn)
        var students = new Dictionary<Guid, GetUserByIdAndRoleIntegrationResponse>();
        foreach (var studentId in studentIds)
        {
            var studentDto =
                await _userScheduleService.GetUserByIdAndRoleAsync("student", studentId, cancellationToken);
            if (studentDto != null) students[studentId] = studentDto;
        }

        // Xử lý Lecturer lookup (nếu LecturerId không được Include từ Schedule)
        var lecturerIds = enrollments.Select(e => e.Schedule.LecturerId).Distinct().ToList();
        var lecturers = new Dictionary<Guid, GetUserByIdAndRoleIntegrationResponse>();
        foreach (var lecturerId in lecturerIds)
        {
            var lecturerDto =
                await _userScheduleService.GetUserByIdAndRoleAsync("Lecturer", lecturerId, cancellationToken);
            if (lecturerDto != null) lecturers[lecturerId] = lecturerDto;
        }

        foreach (var enrollment in enrollments)
        {
            students.TryGetValue(enrollment.StudentId, out var studentDto);
            lecturers.TryGetValue(enrollment.Schedule.LecturerId, out var lecturerDto);

            enrollmentItems.Add(new EnrollmentListItemDto
            {
                EnrollmentId = enrollment.Id,
                EnrollmentDate = enrollment.EnrolledAt,
                StudentId = enrollment.StudentId,
                StudentName = studentDto?.FullName,
                ScheduleId = enrollment.ScheduleId,
                ScheduleName = enrollment.Schedule.Course?.Name,
                CourseId = enrollment.Schedule.CourseId,
                CourseCode = enrollment.Schedule.Course?.Code,
                CourseName = enrollment.Schedule.Course?.Name,
                RoomId = enrollment.Schedule.RoomId,
                RoomName = enrollment.Schedule.Room?.RoomName,
                LecturerId = enrollment.Schedule.LecturerId,
                StartTime = enrollment.Schedule.StartTime,
                EndTime = enrollment.Schedule.EndTime,
                DayOfWeek = enrollment.Schedule.DayOfWeek.ToString(),
                Status = enrollment.Status.ToString()
            });
        }

        // --- 6. Trả về phản hồi ---
        return new GetEnrollmentsResponse
        {
            Items = enrollmentItems,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}