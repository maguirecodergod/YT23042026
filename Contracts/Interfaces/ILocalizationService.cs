using System.Globalization;
using HTT.BlazorWasm.App.Helpers;

namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Enterprise-grade localization service interface.
    /// Supports dot-notation keys, culture switching, hot-reload, and event-driven UI updates.
    /// Supported cultures are auto-discovered from CCountryCode enum metadata.
    /// </summary>
    public interface ILocalizationService
    {
        // ═══════════════════════════════════════════════════════════
        //  Properties
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Quick access via dot-notation key. Example: service["Common.Country.VN"]
        /// Returns the key itself if no translation is found (fallback).
        /// </summary>
        string this[string key] { get; }

        /// <summary>Current active culture.</summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Supported cultures auto-discovered from CCountryCode entries 
        /// that have CultureCode set in their HTTMetadata.
        /// </summary>
        IReadOnlyList<CultureEntry> SupportedCultureEntries { get; }

        /// <summary>
        /// All countries auto-discovered from CCountryCode entries.
        /// Some may have empty CultureCode if they are not yet localized.
        /// </summary>
        IReadOnlyList<CultureEntry> AllCultureEntries { get; }

        /// <summary>Whether the localization data has been loaded for the current culture.</summary>
        bool IsLoaded { get; }

        // ═══════════════════════════════════════════════════════════
        //  Events
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Fired when culture changes or resources are reloaded (hot-reload).
        /// Subscribe in components to trigger StateHasChanged().
        /// </summary>
        event Action? OnLanguageChanged;

        // ═══════════════════════════════════════════════════════════
        //  Core Methods
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize the service: restore persisted culture, preload default resources.
        /// Call once at app startup (e.g., in MainLayout.OnInitializedAsync).
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Get a localized string by dot-notation key.
        /// Returns the key itself as fallback when not found.
        /// </summary>
        string GetString(string key);

        /// <summary>
        /// Get a localized string with formatting arguments.
        /// Example: GetString("Common.Welcome", userName) → "Hello, John!"
        /// </summary>
        string GetString(string key, params object[] args);

        /// <summary>
        /// Switch to a new culture. Persists to localStorage, reloads resources,
        /// and fires OnLanguageChanged.
        /// </summary>
        Task SetCultureAsync(string cultureName);

        // ═══════════════════════════════════════════════════════════
        //  Resource Management
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Preload one or more resource files into cache.
        /// Example: PreloadAsync("Common", "Dashboard")
        /// </summary>
        Task PreloadAsync(params string[] resourceNames);

        /// <summary>
        /// Force reload all cached resources for the current culture.
        /// Used for hot-reload scenarios.
        /// </summary>
        Task ReloadAsync();

        /// <summary>
        /// Start background polling for resource file changes (hot-reload).
        /// Only active in development. Interval in milliseconds.
        /// </summary>
        Task StartHotReloadAsync(int intervalMs = 2000);

        /// <summary>Stop the hot-reload polling.</summary>
        void StopHotReload();
    }
}