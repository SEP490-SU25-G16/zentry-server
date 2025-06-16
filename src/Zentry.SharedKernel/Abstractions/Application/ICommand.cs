namespace Zentry.SharedKernel.Abstractions.Application;

public interface ICommand
{
}

public interface ICommand<out TResponse> : ICommand
{
}