namespace Zentry.Modules.UserManagement.Features.GetUsers;

public class GetUsersResponse
{
    public IEnumerable<UserListItemDto> Users { get; set; } = new List<UserListItemDto>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
