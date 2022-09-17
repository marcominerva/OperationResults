using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OperationResults.Sample.DataAccessLayer.Entities;

namespace OperationResults.Sample.DataAccessLayer.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.FirstName).HasMaxLength(30).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(50);
    }
}
