using System.Reflection;

namespace HTT.BlazorWasm.App.Helpers
{
    public static class EnumExtensions
    {
        public static HTTMetadataAttribute? GetMetadata(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return null;

            return type.GetField(name)?
                       .GetCustomAttribute<HTTMetadataAttribute>();
        }

        public static string GetIcon(this Enum value)
        {
            return value.GetMetadata()?.Icon ?? string.Empty;
        }

        /// <summary>
        /// Gets the raw Name from HTTMetadata (which is now a localization key).
        /// Use GetLocalizedName() for the translated string.
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            return value.GetMetadata()?.Name ?? value.ToString();
        }

        /// <summary>
        /// Gets the localized display name by resolving the HTTMetadata.Name 
        /// through the ILocalizationService.
        /// </summary>
        public static string GetLocalizedName(this Enum value, ILocalizationService localization)
        {
            var key = value.GetMetadata()?.Name;
            if (string.IsNullOrEmpty(key))
                return value.ToString();

            var localized = localization.GetString(key);

            // If the service returns the key itself (not yet loaded), fallback to Description
            if (localized == key)
            {
                return value.GetMetadata()?.Description ?? value.ToString();
            }

            return localized;
        }

        public static string GetDescription(this Enum value)
        {
            return value.GetMetadata()?.Description ?? string.Empty;
        }

        /// <summary>
        /// Gets the CultureCode from HTTMetadata (e.g., "en-US", "vi-VN").
        /// Returns null if no CultureCode is defined.
        /// </summary>
        public static string? GetCultureCode(this Enum value)
        {
            return value.GetMetadata()?.CultureCode;
        }

        /// <summary>
        /// Gets all enum values with their localized metadata.
        /// Useful for building dropdown lists / select components.
        /// </summary>
        public static IEnumerable<EnumItem<T>> GetLocalizedItems<T>(
            this ILocalizationService localization) where T : struct, Enum
        {
            return Enum.GetValues<T>().Select(value =>
            {
                var metadata = (value as Enum).GetMetadata();
                var key = metadata?.Name ?? string.Empty;
                var displayName = !string.IsNullOrEmpty(key)
                    ? localization.GetString(key)
                    : value.ToString();

                // Fallback if key returned as-is
                if (displayName == key && metadata?.Description != null)
                    displayName = metadata.Description;

                return new EnumItem<T>(
                    Value: value,
                    DisplayName: displayName,
                    Icon: metadata?.Icon ?? string.Empty,
                    Description: metadata?.Description ?? string.Empty,
                    CultureCode: metadata?.CultureCode
                );
            });
        }

        /// <summary>
        /// Discovers all supported cultures from an enum type's CultureCode metadata.
        /// Example: EnumExtensions.GetSupportedCultures&lt;CCountryCode&gt;()
        /// Returns entries that have a non-null CultureCode.
        /// </summary>
        public static IReadOnlyList<CultureEntry> GetSupportedCultures<T>() where T : struct, Enum
        {
            return Enum.GetValues<T>()
                .Select(value =>
                {
                    var metadata = (value as Enum).GetMetadata();
                    return metadata?.CultureCode != null
                        ? new CultureEntry(
                            CultureCode: metadata.CultureCode,
                            Icon: metadata.Icon ?? string.Empty,
                            DisplayName: metadata.Name ?? value.ToString(),
                            Description: metadata.Description ?? string.Empty,
                            CountryCode: value
                        )
                        : null;
                })
                .Where(entry => entry != null)
                .Cast<CultureEntry>()
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Represents a fully resolved enum item with localized display data.
    /// </summary>
    public sealed record EnumItem<T>(
        T Value,
        string DisplayName,
        string Icon,
        string Description,
        string? CultureCode = null
    ) where T : struct, Enum;

    /// <summary>
    /// Represents a discovered culture entry from enum metadata.
    /// </summary>
    public sealed record CultureEntry(
        string CultureCode,
        string Icon,
        string DisplayName,
        string Description,
        object CountryCode
    );
}
