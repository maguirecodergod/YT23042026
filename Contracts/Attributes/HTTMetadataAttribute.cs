namespace HTT.BlazorWasm.App.Contracts
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HTTMetadataAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public string? HexColor { get; set; }

        /// <summary>
        /// Optional BCP-47 culture code (e.g., "en-US", "vi-VN").
        /// Used on CCountryCode entries that have localization support
        /// to dynamically discover supported cultures.
        /// </summary>
        public string? CultureCode { get; set; }

        public HTTMetadataAttribute(
            string? name = null,
            string? icon = null,
            string? description = null,
            string? hexColor = null,
            string? cultureCode = null)
        {
            Name = name;
            Icon = icon;
            Description = description;
            HexColor = hexColor;
            CultureCode = cultureCode;
        }
    }
}
