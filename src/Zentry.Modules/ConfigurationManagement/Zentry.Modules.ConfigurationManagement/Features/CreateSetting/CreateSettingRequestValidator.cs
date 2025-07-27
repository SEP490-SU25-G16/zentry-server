using FluentValidation;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Domain;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateSetting;

public class CreateSettingRequestValidator : BaseValidator<CreateSettingRequest>
{
    public CreateSettingRequestValidator()
    {
        RuleFor(x => x.AttributeDefinitionDetails)
            .NotNull()
            .WithMessage(ErrorMessages.Settings.AttributeDefinitionRequired);

        RuleFor(x => x.Setting)
            .NotNull()
            .WithMessage(ErrorMessages.Settings.SettingDetailsRequired);

        When(x => x.AttributeDefinitionDetails != null, () =>
        {
            RuleFor(x => x.AttributeDefinitionDetails.Key)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.KeyRequired)
                .MaximumLength(100)
                .WithMessage("Key không được vượt quá 100 ký tự");

            RuleFor(x => x.AttributeDefinitionDetails.DisplayName)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.DisplayNameRequired)
                .MaximumLength(200)
                .WithMessage("Tên hiển thị không được vượt quá 200 ký tự");

            RuleFor(x => x.AttributeDefinitionDetails.DataType)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.DataTypeRequired);

            RuleFor(x => x.AttributeDefinitionDetails.ScopeType)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.ScopeTypeRequired);
        });

        When(x => x.Setting != null, () =>
        {
            RuleFor(x => x.Setting.ScopeType)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.SettingScopeTypeRequired);

            RuleFor(x => x.Setting.ScopeId)
                .NotEqual(Guid.Empty)
                .WithMessage(ErrorMessages.Settings.ScopeIdRequired);

            RuleFor(x => x.Setting.Value)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.ValueRequired);
        });
    }
}
