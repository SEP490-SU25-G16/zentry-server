using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.UserManagement.Persistence;
using Zentry.Modules.UserManagement.Services;
// using Mailgun.Net.Extensions; // New: For Mailgun DI

namespace Zentry.Modules.UserManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.UserManagement")
            ));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register services (Real implementations)
        services.AddTransient<IJwtService, JwtService>(); // Real JWT Service
        services.AddTransient<IEmailService, SendGridEmailService>(); // NEW: Use SendGrid Email Service
        services.AddTransient<IArgon2PasswordHasher, Argon2PasswordHasher>(); // Argon2 Password Hasher


        return services;
    }
}

// Optional: Add a validation pipeline behavior for MediatR
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);
        var context = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
