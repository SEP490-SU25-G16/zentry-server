using FluentValidation;
using Zentry.SharedKernel.Abstractions.Models;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Rooms.UpdateRoom;

public class UpdateRoomRequestValidator : BaseValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        // Rule for RoomName
        RuleFor(x => x.RoomName)
            .NotEmpty()
            .WithMessage("Tên phòng là bắt buộc.")
            .MaximumLength(100)
            .WithMessage("Tên phòng không được vượt quá 100 ký tự.");

        // Rule for Building
        RuleFor(x => x.Building)
            .NotEmpty()
            .WithMessage("Tên tòa nhà là bắt buộc.")
            .MaximumLength(100)
            .WithMessage("Tên tòa nhà không được vượt quá 100 ký tự.");

        // Rule for Capacity
        RuleFor(x => x.Capacity)
            .NotEmpty()
            .WithMessage("Sức chứa là bắt buộc.")
            .GreaterThan(0)
            .WithMessage("Sức chứa phải lớn hơn 0.")
            .LessThanOrEqualTo(1000)
            .WithMessage("Sức chứa không được vượt quá 1000.");
    }
}
