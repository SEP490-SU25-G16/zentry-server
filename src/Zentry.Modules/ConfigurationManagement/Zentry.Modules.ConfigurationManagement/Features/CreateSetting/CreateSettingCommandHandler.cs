using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateSetting;

public class CreateSettingCommandHandler(
    IAttributeService attributeService,
    ConfigurationDbContext dbContext,
    ILogger<CreateSettingCommandHandler> logger)
    : ICommandHandler<CreateSettingCommand, CreateSettingResponse>
{
    public async Task<CreateSettingResponse> Handle(CreateSettingCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            AttributeDefinition attributeDefinition;
            List<Option> createdOrUpdatedOptions = [];

            if (command.AttributeDefinitionDetails != null)
            {
                DataType attributeDefinitionDataType;
                ScopeType attributeDefinitionScopeType;
                try
                {
                    attributeDefinitionDataType = DataType.FromName(command.AttributeDefinitionDetails.DataType);
                    attributeDefinitionScopeType = ScopeType.FromName(command.AttributeDefinitionDetails.ScopeType);
                }
                catch (InvalidOperationException ex) // Enumeration.FromName ném ArgumentException
                {
                    logger.LogWarning(ex, "Invalid DataType or ScopeType provided for Attribute Definition: {Message}",
                        ex.Message);
                    // Thay đổi dòng này
                    throw new InvalidAttributeDefinitionTypeException(ErrorMessages.Settings
                        .InvalidAttributeDefinitionDataTypeOrScopeType);
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
                        logger.LogWarning("Attribute Definition with Key '{Key}' already exists with a different ID.",
                            command.AttributeDefinitionDetails.Key);
                        throw new AttributeDefinitionKeyAlreadyExistsException(command.AttributeDefinitionDetails.Key);
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

                if (attributeDefinition.DataType == DataType.Selection)
                {
                    if (command.AttributeDefinitionDetails.Options != null &&
                        command.AttributeDefinitionDetails.Options.Count != 0)
                    {
                        var oldOptions = await dbContext.Options
                            .Where(o => o.AttributeId == attributeDefinition.Id)
                            .ToListAsync(cancellationToken);
                        if (oldOptions.Count != 0)
                        {
                            dbContext.Options.RemoveRange(oldOptions);
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }

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
                    else // for DataType.Selection without options
                    {
                        logger.LogWarning(
                            "Attribute Definition with DataType 'Selection' must have options provided for Key '{Key}'.",
                            attributeDefinition.Key);
                        throw new SelectionDataTypeRequiresOptionsException();
                    }
                }
            }
            else
            {
                logger.LogWarning(
                    "AttributeDefinitionDetails is required to create or update an Attribute Definition.");
                throw new ArgumentException(
                    "AttributeDefinitionDetails is required to create or update an Attribute Definition.");
            }

            ScopeType settingScopeType;
            try
            {
                settingScopeType = ScopeType.FromName(command.Setting.ScopeType);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Invalid ScopeType provided for Setting: {Message}", ex.Message);
                throw new InvalidSettingValueException(ErrorMessages.Settings
                    .InvalidSettingValue);
            }

            if (!await attributeService.IsValueValidForAttribute(attributeDefinition.Id, command.Setting.Value))
            {
                logger.LogWarning("Provided value '{Value}' is not valid for Attribute '{Key}' (DataType: {DataType}).",
                    command.Setting.Value, attributeDefinition.Key, attributeDefinition.DataType);
                throw new InvalidSettingValueException(ErrorMessages.Settings
                    .InvalidSettingValue);
            }

            if (!Guid.TryParse(command.Setting.ScopeId, out var parsedScopeId))
            {
                throw new ArgumentException("ScopeId không phải là định dạng GUID hợp lệ.");
            }

            var existingSetting = await dbContext.Settings
                .FirstOrDefaultAsync(c => c.AttributeId == attributeDefinition.Id &&
                                          c.ScopeType == settingScopeType &&
                                          c.ScopeId == parsedScopeId, cancellationToken);

            if (existingSetting != null)
            {
                logger.LogWarning(
                    "Setting for Attribute '{Key}' with Scope '{ScopeType}' and ScopeId '{ScopeId}' already exists.",
                    attributeDefinition.Key, settingScopeType, parsedScopeId);
                // Thay đổi dòng này
                throw new SettingAlreadyExistsException(attributeDefinition.Key, settingScopeType.ToString(),
                    parsedScopeId);
            }

            var newSetting = Setting.Create(
                attributeDefinition.Id,
                settingScopeType,
                parsedScopeId,
                command.Setting.Value
            );

            await dbContext.Settings.AddAsync(newSetting, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var optionDtos = createdOrUpdatedOptions.Select(o => new OptionDto
            {
                Id = o.Id,
                Value = o.Value,
                DisplayLabel = o.DisplayLabel,
                SortOrder = o.SortOrder
            }).ToList();

            logger.LogInformation(
                "Setting {SettingId} created successfully for Attribute '{AttributeKey}' with Scope '{ScopeType}' and ScopeId '{ScopeId}'.",
                newSetting.Id, attributeDefinition.Key, newSetting.ScopeType, newSetting.ScopeId);

            return new CreateSettingResponse
            {
                SettingId = newSetting.Id,
                AttributeId = attributeDefinition.Id,
                AttributeKey = attributeDefinition.Key,
                AttributeDisplayName = attributeDefinition.DisplayName,
                DataType = attributeDefinition.DataType,
                AttributeDefinitionScopeType = attributeDefinition.ScopeType,
                Unit = attributeDefinition.Unit,
                Options = optionDtos.Any() ? optionDtos : null,
                SettingScopeType = newSetting.ScopeType,
                ScopeId = newSetting.ScopeId,
                Value = newSetting.Value,
                CreatedAt = newSetting.CreatedAt,
                UpdatedAt = newSetting.UpdatedAt
            };
        }
        catch (BusinessLogicException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex,
                "An unexpected error occurred while creating setting for attribute '{AttributeKey}' and scope '{ScopeType}' '{ScopeId}'.",
                command.AttributeDefinitionDetails?.Key, command.Setting?.ScopeType,
                command.Setting?.ScopeId);
            throw;
        }
    }
}
