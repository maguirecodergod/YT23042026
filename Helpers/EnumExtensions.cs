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

        public static string GetDisplayName(this Enum value)
        {
            return value.GetMetadata()?.Name ?? value.ToString();
        }

        public static string GetDescription(this Enum value)
        {
            return value.GetMetadata()?.Description ?? string.Empty;
        }
    }
}
