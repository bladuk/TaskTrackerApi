using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("*", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
    });

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
        options.UseAsyncSeeding(async (context, _, ct) =>
        {
            var dbContext = (TaskTrackerDbContext)context;

            if (!await dbContext.Projects.AnyAsync(ct))
            {
                Project project = new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Default Project",
                    Description = "A sample project for testing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                dbContext.Projects.Add(project);

                dbContext.Tasks.Add(new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Default Task",
                    Description = "A sample task for testing",
                    IsCompleted = false,
                    ProjectId = project.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
                Log.Information("Seeding completed successfully");
            }
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(config =>
    {
        config.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Tracker API Reference", Version = "v1" });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSerilogRequestLogging();
    app.MapControllers();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}