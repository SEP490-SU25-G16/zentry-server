using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;
using Zentry.SharedKernel.Contracts.Schedule; // Có thể cần nếu dùng integration query

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetClassDetail;

public class GetClassDetailQueryHandler(
    IScheduleRepository scheduleRepository,
    IEnrollmentRepository enrollmentRepository,
    IMediator mediator // Mediator vẫn có thể cần nếu CountActiveStudentsByClassSectionIdIntegrationQuery là cần thiết
) : IQueryHandler<GetClassDetailQuery, ClassDetailDto>
{
    public async Task<ClassDetailDto> Handle(GetClassDetailQuery request, CancellationToken cancellationToken)
    {
        // Lấy Schedule liên quan đến ClassSectionId.
        // Một ClassSection có thể có nhiều Schedules nếu nó diễn ra vào các ngày/giờ khác nhau.
        // Tuy nhiên, để tính duration và building, chúng ta thường chỉ cần một Schedule đại diện
        // hoặc tính toán dựa trên tất cả các schedules.
        // Giả sử chúng ta lấy Schedule đầu tiên hoặc đại diện để lấy thông tin cơ bản.
        // Hoặc bạn có thể tạo một query mới trong IScheduleRepository để lấy Course và Room trực tiếp từ ClassSection.

        // Cách 1: Query qua Schedule để có Course và Room (giả sử ClassSection có ít nhất một Schedule)
        // Chúng ta cần một projection từ ScheduleRepository để không load toàn bộ object.
        var scheduleInfo = await scheduleRepository.GetScheduleDetailsForClassSectionAsync(
            request.ClassSectionId, cancellationToken);

        if (scheduleInfo == null)
        {
            // Nếu không tìm thấy schedule nào cho class section này, có thể ném NotFound
            throw new NotFoundException("ClassSection", request.ClassSectionId);
        }

        // Lấy số lượng sinh viên đăng ký
        // Sử dụng Integration Query hiện có
        var totalStudentsCountResponse = await mediator.Send(
            new CountActiveStudentsByClassSectionIdIntegrationQuery(request.ClassSectionId),
            cancellationToken);

        // Tính Duration: Giả sử duration là sự khác biệt giữa EndTime và StartTime của schedule.
        // Nếu có nhiều schedules, bạn có thể lấy duration của schedule đầu tiên hoặc trung bình.
        // Ở đây, tôi sẽ dùng của scheduleInfo lấy được.
        var duration = scheduleInfo.EndTime - scheduleInfo.StartTime;

        return new ClassDetailDto
        {
            ClassSectionId = request.ClassSectionId,
            CourseName = scheduleInfo.CourseName,
            EnrolledStudentsCount = totalStudentsCountResponse.TotalStudents,
            DurationInMinutes = (int)duration.TotalMinutes, // Chuyển đổi sang phút
            Building = scheduleInfo.Building
        };
    }
}
