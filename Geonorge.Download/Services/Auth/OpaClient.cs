using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Geonorge.Download.Services.Auth
{
    /// <param name="ValidToken">Token is active, issued for this API and from an allowed client (otherwise respond 401).</param>
    /// <param name="CanOrder">The Altinn PDP permits the user to download for at least one organisation (otherwise respond 403).</param>
    /// <param name="RoleSufficient">Every required entry has no role or lists the download role.</param>
    public record OpaDecision(
        [property: JsonPropertyName("valid_token")] bool ValidToken,
        [property: JsonPropertyName("can_order")] bool CanOrder,
        [property: JsonPropertyName("role_sufficient")] bool RoleSufficient)
    {
        public bool Allowed => ValidToken && CanOrder && RoleSufficient;
    }

    public interface IOpaClient
    {
        /// <summary>Evaluates data.download.decision for the token and the roles required by the order.</summary>
        Task<OpaDecision> GetDecisionAsync(string token, IReadOnlyDictionary<string, List<string>>? requiredRoles, CancellationToken cancellationToken = default);

        /// <summary>Evaluates data.download.valid_token.</summary>
        Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default);
    }

    /// <summary>Thrown when OPA cannot be reached or does not answer successfully.</summary>
    public class OpaUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

    public class OpaClient(HttpClient httpClient, IConfiguration config, ILogger<OpaClient> logger) : IOpaClient
    {
        private const string PolicyPackage = "download";

        private readonly string _baseUrl = (config["OPA_URL"] ?? "http://localhost:3010").TrimEnd('/');

        public async Task<OpaDecision> GetDecisionAsync(string token, IReadOnlyDictionary<string, List<string>>? requiredRoles, CancellationToken cancellationToken = default) =>
            await QueryAsync<OpaDecision>("decision", new { token, requiredRoles = requiredRoles?.ToDictionary(kv => kv.Key, kv => new { role = kv.Value }) }, cancellationToken)
            ?? throw new OpaUnavailableException("OPA returned no decision");

        public async Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default) =>
            await QueryAsync<bool?>("valid_token", new { token }, cancellationToken) == true;

        private async Task<T?> QueryAsync<T>(string rule, object input, CancellationToken cancellationToken)
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

                var result = await response.Content.ReadFromJsonAsync<OpaResult<T>>(cancellationToken);
                return result is null ? default : result.Result;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogError(ex, "OPA request for rule {Rule} failed", rule);
                throw new OpaUnavailableException("OPA request failed", ex);
            }
        }

        private sealed record OpaResult<T>([property: JsonPropertyName("result")] T? Result);
    }
}
