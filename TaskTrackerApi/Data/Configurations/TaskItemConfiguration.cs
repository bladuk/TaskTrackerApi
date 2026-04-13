using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("ProjectTasks");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.IsCompleted).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.Property(p => p.UpdatedAt).IsRequired().HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.IsCompleted);
    }
}