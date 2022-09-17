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

    public IQueryable<T> GetData<T>(bool trackingChanges = false) where T : class
    {
        var set = Set<T>();
        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public void Insert<T>(T entity) where T : class => Set<T>().Add(entity);

    public void Delete<T>(T entity) where T : class => Set<T>().Remove(entity);

    public Task SaveAsync() => SaveChangesAsync();
}
