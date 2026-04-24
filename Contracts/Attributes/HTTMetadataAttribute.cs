using System;

namespace HTT.BlazorWasm.App.Contracts
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HTTMetadataAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public string? HexColor { get; set; }

        public HTTMetadataAttribute(string? name = null, string? icon = null, string? description = null, string? hexColor = null)
        {
            Name = name;
            Icon = icon;
            Description = description;
            HexColor = hexColor;
        }
    }
}
