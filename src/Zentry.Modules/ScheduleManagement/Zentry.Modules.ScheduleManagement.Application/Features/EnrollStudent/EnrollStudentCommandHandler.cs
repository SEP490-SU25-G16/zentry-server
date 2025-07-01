using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IScheduleRepository scheduleRepository, // Thêm
    IUserScheduleService userLookupService, // Thêm
    IMediator mediator)
    : ICommandHandler<EnrollStudentCommand, EnrollmentResponse>
{
    // Inject IScheduleRepository
    // Inject IUserLookupService
    private readonly IMediator _mediator = mediator; // Cho Domain Events và logging

    public async Task<EnrollmentResponse> Handle(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        // --- 3. Kiểm tra StudentId tồn tại và có vai trò Student ---
        var studentUser = await userLookupService.GetByIdAsync(command.StudentId, cancellationToken);
        if (studentUser == null) // Giả định UserLookupDto có IsStudent
            throw new NotFoundException("Student", command.StudentId); // Lỗi 404

        // --- 4. Kiểm tra sinh viên chưa được ghi danh vào Schedule này ---
        var alreadyEnrolled =
            await enrollmentRepository.ExistsAsync(command.StudentId, command.ScheduleId, cancellationToken);
        if (alreadyEnrolled) throw new BusinessLogicException("Student already enrolled in this schedule."); // Lỗi 400

        // --- 5. Tạo bản ghi Enrollment mới ---
        var enrollment = Enrollment.Create(command.StudentId, command.ScheduleId);

        // --- 6. Thêm vào Repository ---
        await enrollmentRepository.AddAsync(enrollment, cancellationToken);

        // --- 7. Lưu thay đổi vào Database ---
        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        // --- 8. (Tùy chọn) Ghi log hoạt động hoặc phát hành Domain Event ---
        // await _mediator.Publish(new StudentEnrolledEvent(enrollment.Id, enrollment.StudentId, enrollment.ScheduleId), cancellationToken);
        // Logger.LogInformation($"Student {command.StudentId} enrolled in Schedule {command.ScheduleId} by Admin {command.AdminId}");


        // --- 9. Trả về thông tin ghi danh ---
        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.Id,
            ScheduleId = enrollment.ScheduleId,
            StudentId = enrollment.StudentId,
            StudentName = studentUser.Name ?? "Unknown Student", // Lấy tên từ UserLookupService
            EnrollmentDate = enrollment.EnrolledAt,
            Status = enrollment.Status.ToString()
        };
    }
}