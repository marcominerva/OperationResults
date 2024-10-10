using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OperationResults.Example.DataAccessLayer.Entities;

namespace OperationResults.Example.DataAccessLayer;

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
