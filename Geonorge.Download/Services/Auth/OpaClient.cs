using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Geonorge.Download.Services.Auth
{
    public interface IOpaClient
    {
        /// <summary>Token is active, issued for this API and from an allowed client.</summary>
        Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>Token is valid and the user is permitted to download via the Altinn PDP for at least one organisation.</summary>
        Task<bool> CanOrderAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Token is valid, the user may order, and every item's access level is allowed.
        /// Items with null or empty access are always allowed.
        /// </summary>
        Task<bool> CanPlaceOrderAsync(string token, string? allowedAccess, IEnumerable<string?> itemAccess, CancellationToken cancellationToken = default);
    }

    /// <summary>Thrown when OPA cannot be reached or does not answer successfully.</summary>
    public class OpaUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

    public class OpaClient(HttpClient httpClient, IConfiguration config, ILogger<OpaClient> logger) : IOpaClient
    {
        private const string PolicyPackage = "download";

        private readonly string _baseUrl = (config["OPA_URL"] ?? "http://localhost:3010").TrimEnd('/');

        public Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default) =>
            QueryAsync("valid_token", new { token }, cancellationToken);

        public Task<bool> CanOrderAsync(string token, CancellationToken cancellationToken = default) =>
            QueryAsync("can_order", new { token }, cancellationToken);

        public Task<bool> CanPlaceOrderAsync(string token, string? allowedAccess, IEnumerable<string?> itemAccess, CancellationToken cancellationToken = default) =>
            QueryAsync("can_place_order", new
            {
                token,
                allowed_access = allowedAccess,
                items = itemAccess.Select(access => new { access }).ToArray()
            }, cancellationToken);

        private async Task<bool> QueryAsync(string rule, object input, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await httpClient.PostAsync(
                    $"{_baseUrl}/v1/data/{PolicyPackage}/{rule}", JsonContent.Create(new { input }), cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("OPA rule {Rule} returned {StatusCode}", rule, response.StatusCode);
                    throw new OpaUnavailableException($"OPA returned {(int)response.StatusCode}");
                }

                var decision = await response.Content.ReadFromJsonAsync<OpaDecision>(cancellationToken);
                return decision?.Result == true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogError(ex, "OPA request for rule {Rule} failed", rule);
                throw new OpaUnavailableException("OPA request failed", ex);
            }
        }

        private sealed record OpaDecision([property: JsonPropertyName("result")] bool? Result);
    }
}
