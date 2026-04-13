using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.Property(p => p.UpdatedAt).IsRequired().HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}