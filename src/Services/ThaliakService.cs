using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface IThaliakService
    {
        public Task<IReadOnlyList<LatestVersionDto>> GetLatestVersionsAsync(CancellationToken cancellationToken = default);
    }

    public sealed class ThaliakService : IThaliakService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ThaliakService>? _logger;
        private readonly IMemoryCache? _cache;
        private readonly string _endPoint;
        private readonly string[] _slugs;

        private const string CacheKey = "Thaliak_LatestVersions";

        private static readonly Dictionary<string, string> SlugNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["2b5cbc63"] = "Boot",
            ["4e9a232b"] = "Base Game",
            ["6b936f08"] = "ex1 (Heavensward)",
            ["f29a3eb2"] = "ex2 (Stormblood)",
            ["859d0e24"] = "ex3 (Shadowbringers)",
            ["1bf99b87"] = "ex4 (Endwalker)",
            ["6cfeab11"] = "ex5 (Dawntrail)"
        };

        public ThaliakService(HttpClient http, ILogger<ThaliakService>? logger = null, IMemoryCache? cache = null)
        {
            _http = http;
            _logger = logger;
            _cache = cache;
            _endPoint = "https://thaliak.xiv.dev/graphql/";
            _slugs = new[]
            {
                "2b5cbc63", // boot
                "4e9a232b", // game
                "6b936f08", // ex1
                "f29a3eb2", // ex2
                "859d0e24", // ex3
                "1bf99b87", // ex4
                "6cfeab11"  // ex5 (Dawntrail)
            };
        }

        public async Task<IReadOnlyList<LatestVersionDto>> GetLatestVersionsAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null && _cache.TryGetValue(CacheKey, out IReadOnlyList<LatestVersionDto>? cached) && cached != null)
            {
                return cached;
            }

            if (_slugs is null || _slugs.Length == 0)
                return Array.Empty<LatestVersionDto>();

            try
            {
                var query = BuildLatestVersionsQuery(_slugs);
                using var doc = await PostGraphQLAsync(query, cancellationToken);

                var data = doc.RootElement.GetProperty("data");
                var list = new List<LatestVersionDto>(_slugs.Length);

                foreach (var prop in data.EnumerateObject())
                {
                    var repo = prop.Value;
                    var slug = repo.GetProperty("slug").GetString()!;
                    var ver = repo.GetProperty("latestVersion").GetProperty("versionString").GetString()!;
                    var name = SlugNameMap.GetValueOrDefault(slug, slug);
                    list.Add(new LatestVersionDto(slug, ver, name));
                }

                _cache?.Set(CacheKey, (IReadOnlyList<LatestVersionDto>)list, TimeSpan.FromMinutes(10));
                return list;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching latest FFXIV versions from Thaliak");
                return Array.Empty<LatestVersionDto>();
            }
        }

        public async Task<IReadOnlyList<string>> GetPatchUrlsAsync()
        {
            if (_slugs is null || _slugs.Length == 0)
                return Array.Empty<string>();

            var query = BuildPatchUrlsQuery(_slugs);
            using var doc = await PostGraphQLAsync(query);

            var data = doc.RootElement.GetProperty("data");
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var repoAlias in data.EnumerateObject())
            {
                var repo = repoAlias.Value;

                if (!repo.TryGetProperty("versions", out var versions) || versions.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var ver in versions.EnumerateArray())
                {
                    if (!ver.TryGetProperty("patches", out var patches) || patches.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var patch in patches.EnumerateArray())
                    {
                        if (patch.TryGetProperty("url", out var urlProp) && urlProp.GetString() is string url && !string.IsNullOrWhiteSpace(url))
                            set.Add(url);
                    }
                }
            }

            return set.ToList();
        }


        private static string BuildLatestVersionsQuery(string[] slugs)
        {
            var sb = new StringBuilder("query{");
            for (int i = 0; i < slugs.Length; i++)
            {
                var slug = slugs[i];
                sb.Append($@"
r{i}: repository(slug:""{slug}"") {{
  slug
  latestVersion {{ versionString }}
}}");
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static string BuildPatchUrlsQuery(string[] slugs)
        {
            var sb = new StringBuilder("query{");
            for (int i = 0; i < slugs.Length; i++)
            {
                var slug = slugs[i];
                sb.Append($@"
r{i}: repository(slug:""{slug}"") {{
  versions {{
    patches {{ url }}
  }}
}}");
            }
            sb.Append('}');
            return sb.ToString();
        }

        private async Task<JsonDocument> PostGraphQLAsync(string query, CancellationToken cancellationToken = default)
        {
            var payload = new { query, variables = (object?)null };

            using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(_endPoint))
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.TryAddWithoutValidation("Accept", "application/json");
            req.Headers.TryAddWithoutValidation("User-Agent", "NetStoneMCP/1.0");

            using var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);

            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{body}");

            var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
            {
                var msg = errors[0].GetProperty("message").GetString();
                throw new InvalidOperationException($"GraphQL error: {msg}\n{body}");
            }

            return doc;
        }
    }
}
