using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SmartJobRunner.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<JobDefinition> JobDefinitions { get; }
    DbSet<JobExecution> JobExecutions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
