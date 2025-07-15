namespace Zentry.SharedKernel.Contracts;

public class ConfigLookupRequestDto
{
    // Key của cấu hình muốn tìm (AttributeKey)
    public string? Key { get; set; }

    // Loại phạm vi (ví dụ: "GLOBAL", "COURSE", "SESSION")
    // Sử dụng string để tránh phụ thuộc vào Smart Enum của Configuration Module
    public string? ScopeType { get; set; }

    // ID của phạm vi. Guid.Empty cho phạm vi GLOBAL hoặc nếu không áp dụng ScopeId cụ thể.
    public Guid? ScopeId { get; set; }

    // Có thể thêm các trường phân trang nếu IConfigurationService hỗ trợ lấy danh sách
    // public int PageNumber { get; set; } = 1;
    // public int PageSize { get; set; } = 1;
}
