using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;
namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments
{
    public class GetEnrollmentsQuery : ICommand<GetEnrollmentsResponse>
    {
        // AdminId sẽ được Controller gán từ JWT để kiểm tra quyền.
        public Guid AdminId { get; set; }

        // Tiêu chí phân trang và lọc
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm (vd: tên sinh viên, tên khóa học)
        public Guid? StudentId { get; set; }
        public Guid? ScheduleId { get; set; }
        public Guid? CourseId { get; set; } // Lọc theo CourseId, từ yêu cầu nghiệp vụ
        public EnrollmentStatus? Status { get; set; } // Lọc theo trạng thái ghi danh
        public string? SortBy { get; set; } // Trường để sắp xếp (vd: "EnrollmentDate", "StudentName")
        public string? SortOrder { get; set; } // "asc" hoặc "desc"
    }
}
