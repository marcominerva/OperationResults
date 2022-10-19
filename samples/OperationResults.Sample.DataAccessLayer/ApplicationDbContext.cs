using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OperationResults.Sample.DataAccessLayer.Entities;

namespace OperationResults.Sample.DataAccessLayer;

public class ApplicationDbContext : DbContext
{
    public DbSet<Person> People { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
