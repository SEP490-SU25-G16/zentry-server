using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
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

        // --- Lấy thông tin Giảng viên từ UserManagement module ---
        GetUserByIdAndRoleIntegrationResponse? lecturerInfo = null;
        if (cs.LecturerId != Guid.Empty)
        {
            var getUserQuery = new GetUserByIdAndRoleIntegrationQuery("Lecturer", cs.LecturerId);
            lecturerInfo = await mediator.Send(getUserQuery, ct);
        }

        // --- Tạo DTO trả về ---
        var response = new ClassSectionDto
        {
            Id = cs.Id,
            CourseId = cs.CourseId,
            CourseCode = cs.Course?.Code,
            CourseName = cs.Course?.Name,
            LecturerId = cs.LecturerId,
            LecturerFullName = lecturerInfo?.FullName,
            LecturerEmail = lecturerInfo?.Email,

            SectionCode = cs.SectionCode,
            Semester = cs.Semester,
            CreatedAt = cs.CreatedAt,
            UpdatedAt = cs.UpdatedAt,
            Schedules = cs.Schedules?
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
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
                .Select(e => new BasicEnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status.ToString()
                })
                .ToList()
        };

        return response;
    }
}
