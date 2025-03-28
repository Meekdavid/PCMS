using Common.ConfigurationSettings;
using Common.Models;
using Core.Results;
using Newtonsoft.Json;
using Persistence.DBContext;
using Persistence.DBModels;
using Polly;
using System.Net;
using System.Text;
using static Common.Literals.StringLiterals;

namespace API.Middlewares
{
    /// <summary>
    /// Middleware for handling exceptions globally, logging them to the database, and returning a structured error response.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        /// <summary>
        /// The next request delegate in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The logger used for logging middleware events and exceptions.
        /// </summary>
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// The host environment to determine the environment the application is running in.
        /// </summary>
        private readonly IHostEnvironment _hostEnvironment;

        /// <summary>
        /// The application database context for logging exceptions to the database.
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="logger">The logger used for logging middleware events and exceptions.</param>
        /// <param name="hostEnvironment">The host environment to determine the environment the application is running in.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IServiceScopeFactory scopeFactory, IHostEnvironment hostEnvironment)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Invokes the middleware to handle exceptions and log them.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Configure retry policy to reprocess request, when an exception is caught
                var retryPolicy = Policy
                .Handle<Exception>() // Catch any exception
                    .WaitAndRetryAsync(ConfigSettings.ApplicationSetting.RetryCountForExceptions, attempt => TimeSpan.FromSeconds(ConfigSettings.ApplicationSetting.SecondsBetweenEachRetry),
                    (exception, timeSpan, retryCount, ctx) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for request {context.Request.Path} due to: {exception.Message}");
                    });

                // Apply retry to the entire request execution
                //await retryPolicy.ExecuteAsync(async () => await _next(context));
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled exception occurred in {GetClassName(ex)} - {GetMethodName(ex)}: {ex.Message}");

                // Log the error to the database
                await LogExceptionToDatabase(ex, context);

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Logs an exception to the database.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LogExceptionToDatabase(Exception exception, HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var errorLog = new ErrorLog
            {
                ErrorLogId = Guid.NewGuid().ToString(),
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Source = GetClassName(exception),
                Method = GetMethodName(exception),
                RequestPath = context.Request.Path,
                RequestBody = await GetRequestBody(context),
                RequestHeaders = JsonConvert.SerializeObject(context.Request.Headers),
                MemberId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : null,
                MemberAgent = context.Request.Headers["Member-Agent"].ToString(),
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                CreatedDate = DateTime.UtcNow,
                IsResolved = false
            };

            try
            {
                _dbContext.ErrorLogs.Add(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save error log to the database.");
            }
        }

        /// <summary>
        /// Retrieves the request body as a string.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The request body as a string.</returns>
        private async Task<string> GetRequestBody(HttpContext context)
        {
            try
            {
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Position = 0; // Reset position
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                    return await reader.ReadToEndAsync();
                }
            }
            catch
            {
                // If request body cannot be read, return empty
            }
            return string.Empty;
        }

        /// <summary>
        /// Handles the exception and returns a structured error response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var isDevelopmentOrTest = _hostEnvironment.IsDevelopment() || _hostEnvironment.IsEnvironment("Test");
            var message = exception.Message ?? "An unexpected error occurred. Please try again later.";
            var details = isDevelopmentOrTest ? exception.StackTrace : null;

            var result = new ErrorDataResult<Exception>(exception, ResponseCode_ExceptionError, message);

            if (!context.Response.HasStarted)
            {
                var json = JsonConvert.SerializeObject(result);
                await context.Response.WriteAsync(json);
            }

        }

        /// <summary>
        /// Retrieves the class name where the exception was caught.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The class name.</returns>
        private string GetClassName(Exception exception)
        {
            // Retrieve class name where exception is caught
            return exception.TargetSite?.DeclaringType?.Name ?? "UnknownClass";
        }

        /// <summary>
        /// Retrieves the method name where the exception was caught.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The method name.</returns>
        private string GetMethodName(Exception exception)
        {
            // Retrieve method name where exception is caught
            return exception.TargetSite?.Name ?? "UnknownMethod";
        }
    }
}
