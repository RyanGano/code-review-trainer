using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace code_review_trainer_service.Services;

/// <summary>
/// Request size and rate limits for the API, in particular the endpoints that call the model.
/// The per-user numbers can be overridden from the "RateLimiting" configuration section.
/// </summary>
public sealed class RateLimitSettings
{
  /// <summary>Review submissions each user may make per hour.</summary>
  public int ReviewsPerUserPerHour { get; set; } = 10;

  /// <summary>Explanation requests each user may make per hour.</summary>
  public int ExplainsPerUserPerHour { get; set; } = 20;

  /// <summary>Model-backed requests the whole service accepts per day, across all users.</summary>
  public int ModelRequestsPerDay { get; set; } = 1000;
}

/// <summary>
/// Marks an endpoint as counting toward the service-wide daily ceiling on model-backed requests.
/// </summary>
public sealed class ModelBackedEndpointMetadata
{
  public static readonly ModelBackedEndpointMetadata Instance = new();

  private ModelBackedEndpointMetadata() { }
}

public static class RequestLimits
{
  public const string ReviewPolicy = "review";
  public const string ExplainPolicy = "explain";

  /// <summary>Largest request body the API accepts, in bytes.</summary>
  public const long MaxRequestBodyBytes = 64 * 1024;

  /// <summary>
  /// Longest review the API accepts. The web app caps the textarea well below this, and the grader
  /// trims anything over its own limit, so this only rejects requests that did not come from the UI.
  /// </summary>
  public const int MaxReviewLength = 8000;

  /// <summary>Longest issue id accepted by the explain endpoint. Real ids are far shorter.</summary>
  public const int MaxIssueIdLength = 100;

  private const string ObjectIdClaim = "oid";
  private const string MappedObjectIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

  /// <summary>
  /// Registers the rate limiter: a per-user hourly policy for each model-backed endpoint, plus a
  /// global daily ceiling that applies only to endpoints marked with
  /// <see cref="ModelBackedEndpointMetadata"/>. Rejections return 429 with a JSON error body.
  /// </summary>
  public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration config)
  {
    var settings = config.GetSection("RateLimiting").Get<RateLimitSettings>() ?? new RateLimitSettings();

    services.AddRateLimiter(options =>
    {
      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

      options.AddPolicy(ReviewPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
          GetPartitionKey(httpContext),
          _ => HourlyWindow(settings.ReviewsPerUserPerHour)));

      options.AddPolicy(ExplainPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
          GetPartitionKey(httpContext),
          _ => HourlyWindow(settings.ExplainsPerUserPerHour)));

      options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        httpContext.GetEndpoint()?.Metadata.GetMetadata<ModelBackedEndpointMetadata>() is null
          ? RateLimitPartition.GetNoLimiter("unmetered")
          : RateLimitPartition.GetFixedWindowLimiter(
              "model-backed",
              _ => new FixedWindowRateLimiterOptions
              {
                PermitLimit = settings.ModelRequestsPerDay,
                Window = TimeSpan.FromDays(1),
                QueueLimit = 0,
                AutoReplenishment = true
              }));

      options.OnRejected = async (context, cancellationToken) =>
      {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
          context.HttpContext.Response.Headers.RetryAfter =
            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
          new { error = "Too many requests. Please wait a while and try again." },
          cancellationToken);
      };
    });

    return services;
  }

  /// <summary>
  /// Applies a per-user rate limit policy and counts the endpoint toward the daily ceiling.
  /// </summary>
  public static TBuilder RequireModelRateLimiting<TBuilder>(this TBuilder builder, string policyName)
    where TBuilder : IEndpointConventionBuilder
  {
    builder.RequireRateLimiting(policyName);
    builder.WithMetadata(ModelBackedEndpointMetadata.Instance);
    return builder;
  }

  /// <summary>
  /// The rate limit partition for a request: the caller's Entra object id when signed in, otherwise
  /// the client address.
  /// </summary>
  public static string GetPartitionKey(HttpContext httpContext)
  {
    var objectId = httpContext.User.FindFirst(ObjectIdClaim)?.Value
      ?? httpContext.User.FindFirst(MappedObjectIdClaim)?.Value;
    if (!string.IsNullOrWhiteSpace(objectId))
    {
      return $"user:{objectId}";
    }

    var clientAddress = GetClientAddress(httpContext);
    return clientAddress is null ? "anonymous" : $"ip:{clientAddress}";
  }

  /// <summary>
  /// The client address as seen by the front end that forwarded the request. Only the last
  /// X-Forwarded-For entry is used: that is the one appended by the platform's own proxy, while
  /// earlier entries are supplied by the client.
  /// </summary>
  public static string? GetClientAddress(HttpContext httpContext)
  {
    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
    if (!string.IsNullOrWhiteSpace(forwardedFor))
    {
      var last = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .LastOrDefault();
      if (!string.IsNullOrEmpty(last))
      {
        // App Service appends "address:port"; drop the port so one client is one partition.
        if (IPEndPoint.TryParse(last, out var endPoint))
        {
          return endPoint.Address.ToString();
        }

        return last;
      }
    }

    return httpContext.Connection.RemoteIpAddress?.ToString();
  }

  private static FixedWindowRateLimiterOptions HourlyWindow(int permitLimit) => new()
  {
    PermitLimit = permitLimit,
    Window = TimeSpan.FromHours(1),
    QueueLimit = 0,
    AutoReplenishment = true
  };
}
