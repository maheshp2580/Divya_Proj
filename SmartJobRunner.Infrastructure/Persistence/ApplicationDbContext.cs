using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Entities;

namespace SmartJobRunner.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<JobDefinition> JobDefinitions => Set<JobDefinition>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<JobDefinition>()
            .HasMany(j => j.Executions)
            .WithOne(e => e.JobDefinition)
            .HasForeignKey(e => e.JobDefinitionId);
    }
}
