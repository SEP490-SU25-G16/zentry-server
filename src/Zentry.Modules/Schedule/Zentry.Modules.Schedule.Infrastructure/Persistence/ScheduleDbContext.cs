using Microsoft.EntityFrameworkCore;
using Zentry.Modules.Schedule.Domain.Entities;

namespace Zentry.Modules.Schedule.Infrastructure.Persistence;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Schedule> Schedules { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScheduleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}