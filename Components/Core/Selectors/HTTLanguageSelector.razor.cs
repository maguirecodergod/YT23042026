namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTLanguageSelector : HTTComponentBase
    {
        private string GetCurrentCountryCode()
        {
            var entry = L.AllCultureEntries.FirstOrDefault(x => x.CultureCode == L.CurrentCulture.Name);
            return entry?.CountryCode.ToString() ?? string.Empty;
        }

        private string GetTooltipText()
        {
            var active = L.AllCultureEntries.FirstOrDefault(x => x.CultureCode == L.CurrentCulture.Name);
            var langName = active != null ? L.GetString(active.DisplayName) : L.CurrentCulture.Name;
            return L.GetString("Layout.Topbar.Language", langName);
        }

        private async Task OnLanguageChanged(string countryCodeStr)
        {
            var entry = L.AllCultureEntries.FirstOrDefault(x => x.CountryCode.ToString() == countryCodeStr);
            if (entry != null && !string.IsNullOrEmpty(entry.CultureCode) && entry.CultureCode != L.CurrentCulture.Name)
            {
                await L.SetCultureAsync(entry.CultureCode);
            }
        }
    }
}
