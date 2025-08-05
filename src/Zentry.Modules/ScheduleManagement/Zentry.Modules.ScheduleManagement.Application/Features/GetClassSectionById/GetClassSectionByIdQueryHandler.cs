// File: Zentry.Modules.ScheduleManagement.Application.Features.GetClassSectionById/GetClassSectionByIdQueryHandler.cs

using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetClassSectionById;

public class GetClassSectionByIdQueryHandler(IClassSectionRepository classSectionRepository, IMediator mediator)
    : IQueryHandler<GetClassSectionByIdQuery, ClassSectionDto>
{
    public async Task<ClassSectionDto> Handle(GetClassSectionByIdQuery query, CancellationToken ct)
    {
        var cs = await classSectionRepository.GetByIdAsync(query.Id, ct);
        if (cs is null || cs.IsDeleted)
            throw new NotFoundException("ClassSection", query.Id);

        // --- 1. Lấy thông tin Giảng viên từ UserManagement module ---
        // Xử lý trường hợp LecturerId có thể là null
        GetUserByIdAndRoleIntegrationResponse? lecturerInfo = null;
        if (cs.LecturerId.HasValue)
        {
            try
            {
                lecturerInfo =
                    await mediator.Send(new GetUserByIdAndRoleIntegrationQuery(Role.Lecturer, cs.LecturerId.Value), ct);
            }
            catch (NotFoundException)
            {
            }
        }

        // --- 2. Lấy danh sách StudentId từ Enrollments ---
        var studentIds = cs.Enrollments?.Select(e => e.StudentId).ToList() ?? new List<Guid>();

        // --- 3. Lấy thông tin chi tiết của tất cả sinh viên bằng Batch Query ---
        var studentInfos = new Dictionary<Guid, BasicUserInfoDto>();
        if (studentIds.Any())
        {
            var usersResponse = await mediator.Send(new GetUsersByIdsIntegrationQuery(studentIds), ct);
            studentInfos = usersResponse.Users.ToDictionary(u => u.Id);
        }

        var response = new ClassSectionDto
        {
            Id = cs.Id,
            CourseId = cs.CourseId,
            CourseCode = cs.Course?.Code,
            CourseName = cs.Course?.Name,
            LecturerId = cs.LecturerId,
            LecturerFullName = lecturerInfo?.FullName ?? "Unassigned Lecturer",
            LecturerEmail = lecturerInfo?.Email,
            SectionCode = cs.SectionCode,
            Semester = cs.Semester,
            CreatedAt = cs.CreatedAt,
            UpdatedAt = cs.UpdatedAt,
            Schedules = cs.Schedules?
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    ClassSectionId = cs.Id,
                    ClassSectionCode = cs.SectionCode,
                    RoomId = s.RoomId,
                    RoomName = s.Room?.RoomName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    WeekDay = s.WeekDay.ToString()
                })
                .ToList(),
            Enrollments = cs.Enrollments?
                .Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.Id,
                    EnrollmentDate = e.EnrolledAt,
                    Status = e.Status.ToString(),
                    StudentId = e.StudentId,
                    StudentName = studentInfos.GetValueOrDefault(e.StudentId)?.FullName ?? "Unknown Student",

                    ClassSectionId = cs.Id,
                    ClassSectionCode = cs.SectionCode,
                    ClassSectionSemester = cs.Semester,

                    CourseId = cs.CourseId,
                    CourseCode = cs.Course?.Code,
                    CourseName = cs.Course?.Name,

                    LecturerId = cs.LecturerId,
                    LecturerName = lecturerInfo?.FullName ?? "Unassigned Lecturer"
                })
                .ToList()
        };

        return response;
    }
}
