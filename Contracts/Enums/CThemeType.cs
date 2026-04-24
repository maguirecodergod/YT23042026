namespace HTT.BlazorWasm.App.Contracts
{
    public enum CThemeType
    {
        [HTTMetadata(Name = "Common.Theme.System", Icon = "bi-circle-half", Description = "Follow system preference")]
        System = 0,

        [HTTMetadata(Name = "Common.Theme.Light", Icon = "bi-sun-fill", Description = "Always use light theme")]
        Light = 1,

        [HTTMetadata(Name = "Common.Theme.Dark", Icon = "bi-moon-stars-fill", Description = "Always use dark theme")]
        Dark = 2
    }
}