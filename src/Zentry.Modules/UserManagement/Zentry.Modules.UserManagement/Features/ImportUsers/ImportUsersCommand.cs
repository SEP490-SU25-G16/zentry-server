using Zentry.Modules.UserManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Features.ImportUsers;

public class ImportUsersCommand : ICommand<ImportUsersResponse>
{
    public ImportUsersCommand(List<UserImportDto> usersToImport)
    {
        UsersToImport = usersToImport;
    }

    public List<UserImportDto> UsersToImport { get; }
}

public class ImportUsersResponse
{
    public bool Success => Errors.Count == 0;
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<ImportError> Errors { get; set; } = new();
}

public class ImportError
{
    public int RowIndex { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
}