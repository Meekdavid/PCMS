using Common.ConfigurationSettings;
using Common.FluentValidators;
using Common.ServiceCollectionExtensions;
using FluentValidation.AspNetCore;
using FluentValidation;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Reflection;
using API.Middlewares;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Persistence.DBData;
using Persistence.DBModels;
using Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Common.Swagger;
using Microsoft.OpenApi.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using BackgroundJobs;
using Hangfire;
using BackgroundJobs.Filter;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Assign configuration to ConfigurationSettingsHelper
    ConfigurationSettingsHelper.Configuration = builder.Configuration;

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
          .ReadFrom.Configuration(builder.Configuration)
          .Enrich.FromLogContext()
          .CreateLogger();

    // 🔹 Configure logging to capture errors
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    // Load environment-specific settings
    var environment = builder.Environment.EnvironmentName; // Gets Development, Production, or Test
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Register FluentValidation
    builder.Services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<MemberRequestValidator>();

    // Register Caching
    builder.Services.AddMemoryCache();

    builder.Host.UseSerilog();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddPCMSServices();// Create a separate IServiceCollection method to register other services


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    // Add Hangfire services
    builder.Services.AddHangfire(config =>
        config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = Environment.ProcessorCount * 2;
        options.Queues = new[] { "default", "contributions", "eligibility", "transactions" };
    });

    // Register background job service
    builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    // Configure Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NLPC PCMS",
            Description = "API documentation for NLPC PCMS system.",
            Version = "v1",
            TermsOfService = new Uri("https://terms-of-service-url.com"),
            Contact = new OpenApiContact
            {
                Name = "Pension Team",
                Email = "support@pcms.com",
                Url = new Uri("https://healthtriagen.com/contact")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            },
            Extensions = new Dictionary<string, IOpenApiExtension>()
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
        //c.OperationFilter<ExamplesOperationFilter>();
        c.ExampleFilters();
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.OperationFilter<ErrorOperationFilter>();

        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    });

}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable CORS
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Initialize recurring jobs
using (var scope = app.Services.CreateScope())
{
    var jobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
    jobService.ScheduleRecurringJobs();
}

// Add Hangfire dashboard (optional)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter(builder.Configuration) },
    StatsPollingInterval = 60000, // 1 minute
    DashboardTitle = "NLPC PCMS Hangfire Dashboard",
    DisplayStorageConnectionString = false, // Hide storage connection string
});


using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var memberManager = serviceProvider.GetRequiredService<UserManager<Member>>();

        // Ensure the database is migrated before seeding
        await context.Database.MigrateAsync();

        // Seed data
        await Initializer.Init(context, roleManager, memberManager, app.Environment);

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("./v1/swagger.json", "NLPC PCMS V1"); });

        app.UseHttpsRedirection();

        app.UseAuthentication(); // Ensure Authentication Middleware is added
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while starting the application.");
    }
}



