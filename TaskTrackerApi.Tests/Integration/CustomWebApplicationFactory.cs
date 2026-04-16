using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Test.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Guid SeedProjectId { get; private set; }
    
    public Guid SeedTaskId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var databaseDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<TaskTrackerDbContext>))
                .ToList();

            foreach (var descriptor in databaseDescriptors)
                services.Remove(descriptor);

            services.AddDbContext<TaskTrackerDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));

            var multiplexerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            
            if (multiplexerDescriptor != null)
                services.Remove(multiplexerDescriptor);

            var databaseMock = new Mock<IDatabase>();
            
            databaseMock
                .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);
            databaseMock
                .Setup(db => db.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(1L);

            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            
            multiplexerMock.Setup(m => m.IsConnected).Returns(false);
            multiplexerMock
                .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(databaseMock.Object);
            services.AddSingleton(multiplexerMock.Object);

            var cacheDescriptors = services
                .Where(d => d.ServiceType == typeof(IDistributedCache))
                .ToList();
            
            foreach (var descriptor in cacheDescriptors)
                services.Remove(descriptor);

            services.AddDistributedMemoryCache();
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        SeedProjectId = Guid.NewGuid();
        SeedTaskId = Guid.NewGuid();

        dbContext.Projects.Add(new Project
        {
            Id = SeedProjectId,
            Name = "Seeded Project",
            Description = "Seeded for integration tests",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.Tasks.Add(new TaskItem
        {
            Id = SeedTaskId,
            Title = "Seeded Task",
            Description = "Seeded for integration tests",
            IsCompleted = false,
            ProjectId = SeedProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    public new Task DisposeAsync() => Task.CompletedTask;
}