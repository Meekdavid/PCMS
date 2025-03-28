using Newtonsoft.Json;
using Persistence.DBContext;
using Persistence.DBModels;
using System.Diagnostics;
using System.Security.Claims;

namespace API.Middlewares
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses, including request/response bodies, headers, and execution time.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        /// <summary>
        /// The next request delegate in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The logger used for logging middleware events.
        /// </summary>
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        /// <summary>
        /// The service scope factory used to create database contexts for logging.
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="logger">The logger used for logging middleware events.</param>
        /// <param name="scopeFactory">The service scope factory used to create database contexts for logging.</param>
        public RequestResponseLoggingMiddleware(RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Invokes the middleware to log request and response details.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            var requestInitiatedAt = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            // Capture Request Data
            var request = await FormatRequest(context.Request);

            // Copy Original Response Body Stream
            var originalResponseBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();
            var responseReceivedAt = DateTime.UtcNow;

            // Capture Response Data
            var response = await FormatResponse(context.Response);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBodyStream);

            // Log to Database
            await LogRequestResponse(context, request, response, stopwatch.Elapsed, requestInitiatedAt, responseReceivedAt);
        }

        /// <summary>
        /// Formats the request body for logging.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The formatted request body as a string.</returns>
        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        /// <summary>
        /// Formats the response body for logging.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <returns>The formatted response body as a string.</returns>
        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        /// <summary>
        /// Logs the request and response details to the database.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="requestBody">The formatted request body.</param>
        /// <param name="responseBody">The formatted response body.</param>
        /// <param name="executionTime">The execution time of the request.</param>
        /// <param name="requestInitiatedAt">The time the request was initiated.</param>
        /// <param name="responseReceivedAt">The time the response was received.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LogRequestResponse(HttpContext context, string requestBody, string responseBody,
            TimeSpan executionTime, DateTime requestInitiatedAt, DateTime responseReceivedAt)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var log = new RequestResponseLog
                {
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = context.Response.StatusCode,
                    RequestHeaders = JsonConvert.SerializeObject(context.Request.Headers),
                    ResponseHeaders = JsonConvert.SerializeObject(context.Response.Headers),
                    ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    MemberAgent = context.Request.Headers["Member-Agent"].ToString() ?? string.Empty,
                    MemberId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                    ExecutionTime = executionTime,
                    RequestInitiatedAt = requestInitiatedAt,
                    ResponseReceivedAt = responseReceivedAt
                };

                await dbContext.RequestResponseLogs.AddAsync(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request/response.");
            }
        }
    }

}
