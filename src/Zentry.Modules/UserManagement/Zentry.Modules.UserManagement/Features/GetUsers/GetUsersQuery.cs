using Zentry.SharedKernel.Abstractions.Application; // Đảm bảo using này có mặt

namespace Zentry.Modules.UserManagement.Features.GetUsers;

public class GetUsersQuery : IQuery<GetUsersResponse>
{
    public int PageNumber { get; init; } = 1; // Mặc định trang 1
    public int PageSize { get; init; } = 10;  // Mặc định 10 người dùng mỗi trang

    // Các trường tùy chọn để lọc
    public string? SearchTerm { get; init; } // Có thể tìm kiếm theo Email, FullName
    public string? Role { get; init; }
    public string? Status { get; init; } // Ví dụ: "Active", "Inactive", "Locked"

    public GetUsersQuery(int pageNumber, int pageSize, string? searchTerm = null, string? role = null, string? status = null)
    {
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        PageSize = pageSize <= 0 ? 10 : pageSize;
        SearchTerm = searchTerm?.Trim();
        Role = role?.Trim();
        Status = status?.Trim();
    }
}
