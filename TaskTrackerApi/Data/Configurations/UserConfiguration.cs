using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Username).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Password).IsRequired().HasMaxLength(255);
        builder.HasIndex(p => p.Username).IsUnique();
    }
}