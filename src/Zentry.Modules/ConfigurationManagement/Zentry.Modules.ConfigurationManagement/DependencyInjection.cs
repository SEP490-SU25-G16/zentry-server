using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zentry.Modules.ConfigurationManagement.Abstractions;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.ConfigurationManagement.Services;

namespace Zentry.Modules.ConfigurationManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigurationInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Zentry.Modules.ConfigurationManagement")
            ));

        // Đã xóa phần đăng ký MediatR và Validators

        services.AddScoped<IAttributeService, AttributeService>();
        return services;
    }
}