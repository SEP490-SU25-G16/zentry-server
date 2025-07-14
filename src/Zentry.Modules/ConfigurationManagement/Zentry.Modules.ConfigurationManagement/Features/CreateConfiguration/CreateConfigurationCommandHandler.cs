using MediatR;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateConfiguration;

public class
    CreateConfigurationCommandHandler(
        IAttributeService attributeService,
        ConfigurationDbContext dbContext,
        IMediator mediator)
    : ICommandHandler<CreateConfigurationCommand, CreateConfigurationResponse>
{
    private readonly IMediator _mediator = mediator;

    public async Task<CreateConfigurationResponse> Handle(CreateConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            AttributeDefinition attributeDefinition;
            List<Option> createdOrUpdatedOptions = [];

            // 1. Xử lý AttributeDefinition
            if (command.AttributeDefinitionDetails != null)
            {
                // Chuyển đổi string DataType và ScopeType từ command/DTO sang Smart Enum
                DataType attributeDefinitionDataType;
                ScopeType attributeDefinitionScopeType;
                try
                {
                    attributeDefinitionDataType = DataType.FromName(command.AttributeDefinitionDetails.DataType);
                    attributeDefinitionScopeType = ScopeType.FromName(command.AttributeDefinitionDetails.ScopeType);
                }
                catch (ArgumentException ex)
                {
                    throw new BusinessLogicException(
                        $"Invalid DataType or ScopeType provided for Attribute Definition: {ex.Message}");
                }

                var existingAttributeDefinition = await dbContext.AttributeDefinitions
                    .FirstOrDefaultAsync(ad => ad.Key == command.AttributeDefinitionDetails.Key, cancellationToken);

                if (existingAttributeDefinition != null)
                {
                    if (command.AttributeDefinitionDetails.Id.HasValue &&
                        existingAttributeDefinition.Id == command.AttributeDefinitionDetails.Id.Value)
                    {
                        existingAttributeDefinition.Update(
                            command.AttributeDefinitionDetails.DisplayName,
                            command.AttributeDefinitionDetails.Description,
                            attributeDefinitionDataType,
                            attributeDefinitionScopeType,
                            command.AttributeDefinitionDetails.Unit
                        );
                        dbContext.AttributeDefinitions.Update(existingAttributeDefinition);
                        attributeDefinition = existingAttributeDefinition;
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            $"Attribute Definition with Key '{command.AttributeDefinitionDetails.Key}' already exists with a different ID.");
                    }
                }
                else
                {
                    attributeDefinition = AttributeDefinition.Create(
                        command.AttributeDefinitionDetails.Key,
                        command.AttributeDefinitionDetails.DisplayName,
                        command.AttributeDefinitionDetails.Description,
                        attributeDefinitionDataType,
                        attributeDefinitionScopeType,
                        command.AttributeDefinitionDetails.Unit
                    );
                    await dbContext.AttributeDefinitions.AddAsync(attributeDefinition, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                // 2. Xử lý Options nếu DataType là Selection
                if (attributeDefinition.DataType == DataType.Selection)
                {
                    if (command.AttributeDefinitionDetails.Options != null &&
                        command.AttributeDefinitionDetails.Options.Any())
                    {
                        // Xóa tất cả options cũ
                        var oldOptions = await dbContext.Options
                            .Where(o => o.AttributeId == attributeDefinition.Id)
                            .ToListAsync(cancellationToken);
                        dbContext.Options.RemoveRange(oldOptions);
                        await dbContext.SaveChangesAsync(cancellationToken);

                        // Tạo options mới
                        foreach (var optionDto in command.AttributeDefinitionDetails.Options)
                        {
                            var newOption = Option.Create(
                                attributeDefinition.Id,
                                optionDto.Value,
                                optionDto.DisplayLabel,
                                optionDto.SortOrder
                            );
                            createdOrUpdatedOptions.Add(newOption);
                            await dbContext.Options.AddAsync(newOption, cancellationToken);
                        }

                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        throw new BusinessLogicException(
                            "Attribute Definition with DataType 'Selection' must have options provided.");
                    }
                }
            }
            else
            {
                throw new BusinessLogicException(
                    "AttributeDefinitionDetails is required to create or update an Attribute Definition.");
            }

            // 3. Chuyển đổi string ScopeType của Configuration từ command/DTO sang Smart Enum
            ScopeType configurationScopeType;
            try
            {
                configurationScopeType = ScopeType.FromName(command.Configuration.ScopeType);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessLogicException($"Invalid ScopeType provided for Configuration: {ex.Message}");
            }

            // 4. Validate Value của Configuration dựa trên DataType của AttributeDefinition
            // QUAN TRỌNG: Phải validate sau khi đã lưu Options (nếu có) vào database
            if (!await attributeService.IsValueValidForAttribute(attributeDefinition.Id, command.Configuration.Value))
                throw new BusinessLogicException(
                    $"Provided value '{command.Configuration.Value}' is not valid for Attribute '{attributeDefinition.DisplayName}' (DataType: {attributeDefinition.DataType}).");

            // 5. Kiểm tra xem cấu hình cho AttributeId, ScopeType, ScopeId đã tồn tại chưa
            var existingConfiguration = await dbContext.Configurations
                .FirstOrDefaultAsync(c => c.AttributeId == attributeDefinition.Id &&
                                          c.ScopeType == configurationScopeType &&
                                          c.ScopeId == command.Configuration.ScopeId, cancellationToken);

            if (existingConfiguration != null)
                throw new BusinessLogicException(
                    $"Configuration for Attribute '{attributeDefinition.Key}' with Scope '{configurationScopeType}' and ScopeId '{command.Configuration.ScopeId}' already exists.");

            // 6. Tạo Configuration entity mới
            var newConfiguration = Configuration.Create(
                attributeDefinition.Id,
                configurationScopeType,
                command.Configuration.ScopeId,
                command.Configuration.Value
            );

            // 7. Thêm Configuration vào DbContext
            await dbContext.Configurations.AddAsync(newConfiguration, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // 8. Hoàn thành giao dịch
            await transaction.CommitAsync(cancellationToken);

            // 9. Trả về Response DTO
            var optionDtos = createdOrUpdatedOptions.Select(o => new OptionDto
            {
                Id = o.Id,
                Value = o.Value,
                DisplayLabel = o.DisplayLabel,
                SortOrder = o.SortOrder
            }).ToList();

            return new CreateConfigurationResponse
            {
                ConfigurationId = newConfiguration.Id,
                AttributeId = attributeDefinition.Id,
                AttributeKey = attributeDefinition.Key,
                AttributeDisplayName = attributeDefinition.DisplayName,
                DataType = attributeDefinition.DataType,
                AttributeDefinitionScopeType = attributeDefinition.ScopeType,
                Unit = attributeDefinition.Unit,
                Options = optionDtos.Any() ? optionDtos : null,
                ConfigurationScopeType = newConfiguration.ScopeType,
                ScopeId = newConfiguration.ScopeId,
                Value = newConfiguration.Value,
                CreatedAt = newConfiguration.CreatedAt,
                UpdatedAt = newConfiguration.UpdatedAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
