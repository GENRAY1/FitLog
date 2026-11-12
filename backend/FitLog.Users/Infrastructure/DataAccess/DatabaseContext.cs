using FitLog.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitLog.Users.Infrastructure.DataAccess;

/// <summary>Контекст БД модуля Users (отдельная база <c>fitlog_users</c>).</summary>
public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
