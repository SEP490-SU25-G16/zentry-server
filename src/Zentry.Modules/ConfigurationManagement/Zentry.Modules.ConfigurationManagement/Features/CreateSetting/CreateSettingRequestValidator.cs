using FluentValidation;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Domain;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;

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

            // THÊM DÒNG NÀY ĐỂ KIỂM TRA CHỈ CHỨA CHỮ
            RuleFor(x => x.AttributeDefinitionDetails.Key)
                .Matches("^[a-zA-Z]+$") // Biểu thức chính quy chỉ cho phép chữ cái (viết hoa và viết thường)
                .WithMessage("Key chỉ được chứa các ký tự chữ cái (a-z, A-Z).");

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

            // Kiểm tra ScopeId là NotEmpty trước
            RuleFor(x => x.Setting.ScopeId)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.ScopeIdRequired);

            // Thêm Rule Must để kiểm tra định dạng GUID
            RuleFor(x => x.Setting!.ScopeId)
                .Must(BeAValidGuid)
                .WithMessage(ErrorMessages.GuidFormatInvalid); // Sử dụng message đã có

            RuleFor(x => x.Setting.Value)
                .NotEmpty()
                .WithMessage(ErrorMessages.Settings.ValueRequired);
        });
    }

    private bool BeAValidGuid(string scopeId)
    {
        return Guid.TryParse(scopeId, out _);
    }
}
