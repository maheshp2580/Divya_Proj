using Hangfire;
using Hangfire.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Infrastructure.AI;
using SmartJobRunner.Infrastructure.BackgroundJobs;
using SmartJobRunner.Infrastructure.Persistence;

namespace SmartJobRunner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IJobSchedulerService, JobSchedulerService>();
        services.AddScoped<IAiAnalysisService, GeminiAnalysisService>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer();

        return services;
    }
}
