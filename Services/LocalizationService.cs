using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using HTT.BlazorWasm.App.Helpers;

namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Enterprise-grade localization service for Blazor WebAssembly.
    /// 
    /// Features:
    /// ─ Supported cultures auto-discovered from CCountryCode enum metadata
    /// ─ Thread-safe concurrent caching with ConcurrentDictionary
    /// ─ Nested JSON → dot-notation flattening (Common.Country.VN)
    /// ─ Multi-resource lazy loading with auto UI re-render after load
    /// ─ Hot-reload via HTTP content-hash polling (no rebuild required)
    /// ─ Culture switching with localStorage persistence
    /// ─ Fallback chain: exact key → Description → key itself
    /// ─ Format string support with params args
    /// ─ Event-driven UI re-rendering via OnLanguageChanged
    /// </summary>
    internal sealed class LocalizationService : ILocalizationService, IDisposable
    {
        // ═══════════════════════════════════════════════════════════
        //  Constants
        // ═══════════════════════════════════════════════════════════

        private const string CultureStorageKey = "htt-culture";
        private const string DefaultCulture = "en-US";
        private const string LocalesBasePath = "locales";

        // ═══════════════════════════════════════════════════════════
        //  Dependencies
        // ═══════════════════════════════════════════════════════════

        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;

        // ═══════════════════════════════════════════════════════════
        //  State
        // ═══════════════════════════════════════════════════════════

        private CultureInfo _currentCulture = new(DefaultCulture);

        /// <summary>
        /// Cache: culture → resourceName → flat dictionary (dot-notation key → value).
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, string>>> _cache = new();

        /// <summary>
        /// Content hash tracking per resource for hot-reload change detection.
        /// Key: "{culture}_{resource}", Value: content hash
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _contentHashes = new();

        /// <summary>
        /// Guards against duplicate concurrent loads of the same resource.
        /// </summary>
        private readonly ConcurrentDictionary<string, Task> _loadingTasks = new();

        /// <summary>
        /// Tracks which resources have been loaded per culture for hot-reload re-fetching.
        /// </summary>
        private readonly ConcurrentDictionary<string, HashSet<string>> _loadedResources = new();

        private CancellationTokenSource? _hotReloadCts;
        private bool _isLoaded;

        // ═══════════════════════════════════════════════════════════
        //  Supported Cultures (auto-discovered from CCountryCode)
        // ═══════════════════════════════════════════════════════════

        private static readonly Lazy<IReadOnlyList<CultureEntry>> _supportedCultures = new(() =>
            EnumExtensions.GetSupportedCultures<CCountryCode>());

        private static readonly Lazy<IReadOnlyList<CultureEntry>> _allCultures = new(() =>
            EnumExtensions.GetAllCultures<CCountryCode>());

        // ═══════════════════════════════════════════════════════════
        //  Constructor
        // ═══════════════════════════════════════════════════════════

        public LocalizationService(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — Properties
        // ═══════════════════════════════════════════════════════════

        public string this[string key] => GetString(key);

        public CultureInfo CurrentCulture => _currentCulture;

        public IReadOnlyList<CultureEntry> SupportedCultureEntries => _supportedCultures.Value;

        public IReadOnlyList<CultureEntry> AllCultureEntries => _allCultures.Value;

        public bool IsLoaded => _isLoaded;

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — Events
        // ═══════════════════════════════════════════════════════════

        public event Action? OnLanguageChanged;

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — Initialize
        // ═══════════════════════════════════════════════════════════

        public async Task InitializeAsync()
        {
            try
            {
                // 1. Restore persisted culture from localStorage
                var saved = await _js.InvokeAsync<string>("localStorage.getItem", CultureStorageKey);

                var validCultures = _supportedCultures.Value.Select(c => c.CultureCode).ToHashSet();

                if (!string.IsNullOrWhiteSpace(saved) && validCultures.Contains(saved))
                {
                    _currentCulture = new CultureInfo(saved);
                }
                else
                {
                    _currentCulture = new CultureInfo(DefaultCulture);
                }

                // 2. Apply culture to the runtime
                ApplyCultureToRuntime(_currentCulture);

                // 3. Load manifest → discover all resource files → preload all
                var resourceNames = await LoadManifestAsync();
                await PreloadAsync([.. resourceNames]);

                _isLoaded = true;
            }
            catch
            {
                // Fallback: use defaults, don't crash the app
                _currentCulture = new CultureInfo(DefaultCulture);
                _isLoaded = true;
            }
        }

        /// <summary>
        /// Loads locales/manifest.json to discover all available resource file names.
        /// Manifest format: ["Common", "Home", "Billing", "Settings"]
        /// Falls back to ["Common"] if manifest is missing or invalid.
        /// </summary>
        private async Task<string[]> LoadManifestAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync($"{LocalesBasePath}/manifest.json");
                var names = JsonSerializer.Deserialize<string[]>(json);
                return names is { Length: > 0 } ? names : ["Common"];
            }
            catch
            {
                return ["Common"];
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — GetString
        // ═══════════════════════════════════════════════════════════

        public string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var culture = _currentCulture.Name;
            var resourceName = ExtractResourceName(key);

            if (_cache.TryGetValue(culture, out var resources))
            {
                // ① Direct lookup: "Home.Abc" → look in resource "Home" only (O(1))
                if (resourceName != null
                    && resources.TryGetValue(resourceName, out var targetResource)
                    && targetResource.TryGetValue(key, out var directValue))
                {
                    return directValue;
                }

                // ② Fallback scan: cross-resource reference or flat key (rare)
                foreach (var resource in resources.Values)
                {
                    if (resource.TryGetValue(key, out var value))
                        return value;
                }
            }

            // ③ Resource not loaded yet → lazy load + notify UI
            if (resourceName != null && !IsResourceLoadedOrLoading(culture, resourceName))
            {
                _ = LazyLoadAndNotifyAsync(culture, resourceName);
            }

            // Return key as placeholder until resource loads
            return key;
        }

        private bool IsResourceLoadedOrLoading(string culture, string resourceName)
        {
            var taskKey = $"{culture}_{resourceName}";
            if (_loadingTasks.ContainsKey(taskKey)) return true;

            if (_loadedResources.TryGetValue(culture, out var loaded))
            {
                lock (loaded) { if (loaded.Contains(resourceName)) return true; }
            }

            return false;
        }

        public string GetString(string key, params object[] args)
        {
            var template = GetString(key);

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — SetCulture
        // ═══════════════════════════════════════════════════════════

        public async Task SetCultureAsync(string cultureName)
        {
            if (_currentCulture.Name == cultureName)
                return;

            var validCultures = _supportedCultures.Value.Select(c => c.CultureCode).ToHashSet();
            if (!validCultures.Contains(cultureName))
                throw new ArgumentException($"Culture '{cultureName}' is not supported. Add CultureCode to the corresponding CCountryCode entry.", nameof(cultureName));

            var newCulture = new CultureInfo(cultureName);

            // 1. Persist to localStorage
            await _js.InvokeVoidAsync("localStorage.setItem", CultureStorageKey, cultureName);

            // 2. Apply to runtime
            _currentCulture = newCulture;
            ApplyCultureToRuntime(newCulture);

            // 3. Collect all previously loaded resources across all cultures
            var resourcesToLoad = new HashSet<string> { "Common" };
            foreach (var kvp in _loadedResources)
            {
                lock (kvp.Value)
                {
                    foreach (var res in kvp.Value)
                        resourcesToLoad.Add(res);
                }
            }

            // 4. Load all for the new culture
            await PreloadAsync([.. resourcesToLoad]);

            // 5. Notify all subscribers
            NotifyChanged();
        }

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — Resource Management
        // ═══════════════════════════════════════════════════════════

        public async Task PreloadAsync(params string[] resourceNames)
        {
            var culture = _currentCulture.Name;
            var tasks = resourceNames
                .Distinct()
                .Select(name => EnsureResourceLoadedAsync(culture, name))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task ReloadAsync()
        {
            var culture = _currentCulture.Name;
            var resources = GetLoadedResourceNames(culture);

            if (resources.Count == 0)
                resources = ["Common"];

            // Clear cache for this culture to force full re-fetch
            _cache.TryRemove(culture, out _);

            // Clear hashes so we detect all as changed
            foreach (var res in resources)
            {
                var hashKey = BuildHashKey(culture, res);
                _contentHashes.TryRemove(hashKey, out _);
            }

            // Reload all
            var tasks = resources.Select(name => LoadResourceAsync(culture, name)).ToArray();
            await Task.WhenAll(tasks);

            NotifyChanged();
        }

        // ═══════════════════════════════════════════════════════════
        //  ILocalizationService — Hot Reload
        // ═══════════════════════════════════════════════════════════

        public async Task StartHotReloadAsync(int intervalMs = 2000)
        {
            StopHotReload();

            _hotReloadCts = new CancellationTokenSource();
            var token = _hotReloadCts.Token;

            // Initial delay before first poll
            await Task.Delay(intervalMs, token).ConfigureAwait(false);

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await PollForChangesAsync(token);
                    }
                    catch (TaskCanceledException) { break; }
                    catch { /* Silently continue polling */ }

                    try
                    {
                        await Task.Delay(intervalMs, token);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, token);
        }

        public void StopHotReload()
        {
            _hotReloadCts?.Cancel();
            _hotReloadCts?.Dispose();
            _hotReloadCts = null;
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — Lazy Load with Notification
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Loads a resource in the background and notifies UI when complete.
        /// This is the key fix for multi-resource support — without notification,
        /// the UI would show the raw key forever after lazy load.
        /// </summary>
        private async Task LazyLoadAndNotifyAsync(string culture, string resourceName)
        {
            await EnsureResourceLoadedAsync(culture, resourceName);
            NotifyChanged();
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — Resource Loading
        // ═══════════════════════════════════════════════════════════

        private Task EnsureResourceLoadedAsync(string culture, string resourceName)
        {
            var taskKey = $"{culture}_{resourceName}";

            return _loadingTasks.GetOrAdd(taskKey, _ =>
                LoadResourceCoreAsync(culture, resourceName, taskKey));
        }

        private async Task LoadResourceCoreAsync(string culture, string resourceName, string taskKey)
        {
            try
            {
                await LoadResourceAsync(culture, resourceName);
            }
            finally
            {
                _loadingTasks.TryRemove(taskKey, out _);
            }
        }

        private async Task LoadResourceAsync(string culture, string resourceName)
        {
            try
            {
                var path = $"{LocalesBasePath}/{culture}/{resourceName}.json";

                // Cache-busting query param to bypass browser HTTP cache
                var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var requestUri = $"{path}?_v={cacheBuster}";

                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    // Even if 404, mark as loaded so we don't spam requests
                    var loaded = _loadedResources.GetOrAdd(culture, _ => []);
                    lock (loaded) { loaded.Add(resourceName); }
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                // Parse and flatten nested JSON
                var flatMap = FlattenJson(json);

                if (flatMap.Count > 0)
                {
                    // Store in cache
                    var cultureCache = _cache.GetOrAdd(culture, _ => new ConcurrentDictionary<string, Dictionary<string, string>>());
                    cultureCache[resourceName] = flatMap;

                    // Track loaded resource
                    var loaded = _loadedResources.GetOrAdd(culture, _ => []);
                    lock (loaded) { loaded.Add(resourceName); }

                    // Store content hash for hot-reload detection
                    var hashKey = BuildHashKey(culture, resourceName);
                    _contentHashes[hashKey] = json.GetHashCode(StringComparison.Ordinal);
                }
            }
            catch
            {
                // Don't crash — cache empty dict so we don't retry endlessly
                var cultureCache = _cache.GetOrAdd(culture, _ => new ConcurrentDictionary<string, Dictionary<string, string>>());
                cultureCache[resourceName] = [];

                // Track as loaded so we don't retry
                var loaded = _loadedResources.GetOrAdd(culture, _ => []);
                lock (loaded) { loaded.Add(resourceName); }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — Hot Reload Polling
        // ═══════════════════════════════════════════════════════════

        private async Task PollForChangesAsync(CancellationToken ct)
        {
            var culture = _currentCulture.Name;
            var resources = GetLoadedResourceNames(culture);
            var hasChanges = false;

            foreach (var resourceName in resources)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var changed = await CheckResourceChangedAsync(culture, resourceName, ct);
                    if (changed)
                    {
                        await LoadResourceAsync(culture, resourceName);
                        hasChanges = true;
                    }
                }
                catch (TaskCanceledException) { throw; }
                catch { /* Skip, try next */ }
            }

            if (hasChanges)
                NotifyChanged();
        }

        private async Task<bool> CheckResourceChangedAsync(string culture, string resourceName, CancellationToken ct)
        {
            var path = $"{LocalesBasePath}/{culture}/{resourceName}.json";
            var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var requestUri = $"{path}?_v={cacheBuster}";

            var response = await _httpClient.GetAsync(requestUri, ct);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync(ct);
            var newHash = json.GetHashCode(StringComparison.Ordinal);
            var hashKey = BuildHashKey(culture, resourceName);

            if (_contentHashes.TryGetValue(hashKey, out var existingHash))
                return existingHash != newHash;

            // No previous hash — treat as changed
            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — JSON Flattening
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Flattens nested JSON into dot-notation keys.
        /// 
        /// Input:  { "Common": { "Country": { "VN": "Việt Nam" } } }
        /// Output: "Common.Country.VN" → "Việt Nam"
        /// 
        /// Supports unlimited nesting depth and multiple resource files:
        ///   Common.json  → Common.Country.VN, Common.Theme.System
        ///   Home.json    → Home.Hero.Title, Home.Hero.Subtitle
        ///   Billing.json → Billing.Invoice.Total, Billing.Plan.Name
        /// </summary>
        private static Dictionary<string, string> FlattenJson(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                FlattenElement(doc.RootElement, string.Empty, result);
            }
            catch
            {
                // Malformed JSON — return empty
            }

            return result;
        }

        private static void FlattenElement(JsonElement element, string prefix, Dictionary<string, string> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = string.IsNullOrEmpty(prefix)
                            ? property.Name
                            : $"{prefix}.{property.Name}";
                        FlattenElement(property.Value, key, result);
                    }
                    break;

                case JsonValueKind.String:
                    result[prefix] = element.GetString() ?? string.Empty;
                    break;

                case JsonValueKind.Number:
                    result[prefix] = element.GetRawText();
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    result[prefix] = element.GetBoolean().ToString();
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        FlattenElement(item, $"{prefix}[{index}]", result);
                        index++;
                    }
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Private — Helpers
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Extracts the resource file name from a dot-notation key.
        /// "Common.Country.VN" → "Common"
        /// "Home.Hero.Title"   → "Home"
        /// "Billing.Plan.Name" → "Billing"
        /// </summary>
        private static string? ExtractResourceName(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var dotIndex = key.IndexOf('.');
            return dotIndex > 0 ? key[..dotIndex] : null;
        }

        private HashSet<string> GetLoadedResourceNames(string culture)
        {
            if (_loadedResources.TryGetValue(culture, out var set))
            {
                lock (set) { return [.. set]; }
            }
            return [];
        }

        private static string BuildHashKey(string culture, string resourceName)
            => $"{culture}_{resourceName}";

        private static void ApplyCultureToRuntime(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        private void NotifyChanged() => OnLanguageChanged?.Invoke();

        // ═══════════════════════════════════════════════════════════
        //  IDisposable
        // ═══════════════════════════════════════════════════════════

        public void Dispose()
        {
            StopHotReload();
        }
    }
}