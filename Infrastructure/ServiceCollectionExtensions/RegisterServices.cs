using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Common.ConfigurationSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Maapings;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence.DBContext;
using Persistence.DBModels;
using Infrastructure.DataAccess.Repositories;
using Application.Interfaces.General;
using Infrastructure.Interfaces;
using Common.Services;
using Application.Interfaces.Database;
using Domain.Interfaces.Database;
using Application.Interfaces.Business;
using Application.Repositories;
using TokenHandler = Common.Services.TokenHandler;
using Infrastructure.Services;

namespace Common.ServiceCollectionExtensions
{
    public static class RegisterServices
    {
        public static IServiceCollection AddPCMSServices(this IServiceCollection services)
        {

            // Register Database Context **FIRST**
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConfigSettings.ConnectionString.DefaultConnection,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: ConfigSettings.ApplicationSetting.RetryCountForDatabaseTransactions,
                            maxRetryDelay: TimeSpan.FromSeconds(ConfigSettings.ApplicationSetting.SecondsBetweenEachRetry),
                            errorNumbersToAdd: null
                        );
                    }
                )
            );

            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddScoped<ITokenHandler, TokenHandler>();
            services.AddTransient<IEmailServiceCustom, EmailService>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IStorageFactory, StorageFactory>();
            services.AddSingleton<ILocalStorage, LocalStorage>();
            services.AddTransient<IContributionManager, ContributionManager>();
            services.AddTransient<ITransactionManager, TransactionManager>();
            services.AddTransient<IAccountManager, AccountManager>();
            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IFirebaseStorage, FirebaseStorage>();
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IBenefitEligibilityRepository, BenefitEligibilityRepository>();
            services.AddTransient<IContributionRepository, ContributionRepository>();
            services.AddTransient<INotificationManager, NotificationManager>();
            services.AddTransient<IBenefitEligibilityManager, BenefitEligibilityManager>();
            services.AddTransient<IEmployerRepository, EmployerRepository>();
            services.AddTransient<IMemberRepository, MemberRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IEmployerManager, EmployerManager>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IUserManager, MemberManager>();
            services.AddTransient<ISMSService, SMSService>();
            services.AddTransient<IEligibilityRuleRepository, EligibilityRuleRepository>();
            services.AddSingleton<ICacheService, CacheService>();


            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });


            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;  // Suppress the default model state invalid filter
            });

            //services.AddDbContext<ApplicationDbContext>(options =>
            //       options.UseSqlServer(ConfigSettings.ConnectionString.DefaultConnection));



            services.AddIdentity<Member, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure JWT Authentication
            //var TEST = ConfigSettings.ApplicationSetting.JwtConfig.Issuer;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = ConfigSettings.ApplicationSetting.JwtConfig.Issuer,
                    ValidAudience = ConfigSettings.ApplicationSetting.JwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigSettings.ApplicationSetting.JwtConfig.SecretKey))
                };
            });

            // Add Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
