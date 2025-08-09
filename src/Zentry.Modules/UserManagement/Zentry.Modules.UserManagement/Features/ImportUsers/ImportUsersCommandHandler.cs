using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Dtos;
using Zentry.Modules.UserManagement.Entities;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

namespace Zentry.Modules.UserManagement.Features.ImportUsers;

// Inject IPasswordHasher vào constructor
public class ImportUsersCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<ImportUsersCommandHandler> logger)
    : ICommandHandler<ImportUsersCommand, ImportUsersResponse>
{
    public async Task<ImportUsersResponse> Handle(ImportUsersCommand command, CancellationToken cancellationToken)
    {
        var response = new ImportUsersResponse();
        var validUsers = new List<UserImportDto>();

        // Lọc các bản ghi rỗng
        var usersToProcess = command.UsersToImport.Where(u => !string.IsNullOrWhiteSpace(u.Email)).ToList();

        // 1. Xác thực và lọc dữ liệu đầu vào
        foreach (var userDto in usersToProcess)
        {
            var validator = new UserImportDtoValidator();
            var validationResult = await validator.ValidateAsync(userDto, cancellationToken);

            if (validationResult.IsValid)
            {
                validUsers.Add(userDto);
            }
            else
            {
                var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                response.Errors.Add(new ImportError
                {
                    RowIndex = userDto.RowIndex,
                    Email = userDto.Email,
                    Message = errorMessage
                });
            }
        }

        // Nếu không có user hợp lệ để xử lý, trả về lỗi ngay
        if (validUsers.Count == 0)
        {
            response.FailedCount = usersToProcess.Count;
            return response;
        }

        // 2. Kiểm tra các email trùng lặp trong hệ thống hiện tại
        var existingEmails = await userRepository.GetExistingEmailsAsync(validUsers.Select(u => u.Email).ToList());
        var duplicateEmailsInInput = validUsers
            .GroupBy(u => u.Email, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        // Lọc lại danh sách users để loại bỏ các bản ghi trùng lặp
        var finalUsersToProcess = new List<UserImportDto>();
        foreach (var userDto in validUsers)
            if (existingEmails.Contains(userDto.Email, StringComparer.OrdinalIgnoreCase))
            {
                response.Errors.Add(new ImportError
                {
                    RowIndex = userDto.RowIndex,
                    Email = userDto.Email,
                    Message = $"Email '{userDto.Email}' đã tồn tại trong hệ thống."
                });
            }
            else if (duplicateEmailsInInput.Contains(userDto.Email, StringComparer.OrdinalIgnoreCase))
            {
                // Chỉ cần ghi lỗi một lần cho các email trùng trong file
                if (!response.Errors.Any(e => e.Email.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase)))
                    response.Errors.Add(new ImportError
                    {
                        RowIndex = userDto.RowIndex, // Ghi lại dòng đầu tiên có lỗi
                        Email = userDto.Email,
                        Message = $"Email '{userDto.Email}' bị trùng lặp trong file import."
                    });
            }
            else
            {
                finalUsersToProcess.Add(userDto);
            }

        var accountsToCreate = new List<Account>();
        var usersToCreate = new List<User>();

        foreach (var userDto in finalUsersToProcess)
            try
            {
                var role = Role.FromName(userDto.Role);
                var (hashedPassword, salt) = passwordHasher.HashPassword(userDto.Password);

                var account = Account.Create(userDto.Email, hashedPassword, salt, role);
                var user = User.Create(account.Id, userDto.FullName, userDto.PhoneNumber);

                accountsToCreate.Add(account);
                usersToCreate.Add(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to prepare user data for import: {Email}", userDto.Email);
                response.Errors.Add(new ImportError
                {
                    RowIndex = userDto.RowIndex,
                    Email = userDto.Email,
                    Message = $"Lỗi khi chuẩn bị dữ liệu: {ex.Message}"
                });
            }

        try
        {
            await userRepository.AddRangeAsync(accountsToCreate, usersToCreate, cancellationToken);
            response.ImportedCount = accountsToCreate.Count;
            response.FailedCount = command.UsersToImport.Count - response.ImportedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save imported users to database.");
            // Ghi lại lỗi chung
            response.Errors.Add(new ImportError
            {
                RowIndex = 0,
                Email = string.Empty,
                Message = $"Lỗi khi lưu vào CSDL: {ex.Message}"
            });
            response.ImportedCount = 0;
            response.FailedCount = command.UsersToImport.Count;
        }

        return response;
    }
}