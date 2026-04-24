using HTT.BlazorWasm.App.Helpers;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTLanguageSelector : HTTComponentBase
    {
        private bool _isOpen;

        private void ToggleDropdown()
        {
            _isOpen = !_isOpen;
        }

        private async Task HandleFocusOut(FocusEventArgs e)
        {
            // Small delay so that the click event on the button item can execute
            // before the dropdown gets hidden and removed from DOM.
            await Task.Delay(150);
            _isOpen = false;
        }

        private string GetActiveIcon()
        {
            var active = L.SupportedCultureEntries.FirstOrDefault(x => x.CultureCode == L.CurrentCulture.Name);
            return active?.Icon ?? "bi bi-translate"; 
        }

        private string GetTooltipText()
        {
            var active = L.SupportedCultureEntries.FirstOrDefault(x => x.CultureCode == L.CurrentCulture.Name);
            var langName = active != null ? L.GetString(active.DisplayName) : L.CurrentCulture.Name;
            return L.GetString("Layout.Topbar.Language", langName);
        }

        private bool IsActive(CultureEntry entry)
        {
            return entry.CultureCode == L.CurrentCulture.Name;
        }

        private async Task SelectLanguageAsync(CultureEntry entry)
        {
            if (!IsActive(entry))
            {
                await L.SetCultureAsync(entry.CultureCode);
            }
            _isOpen = false;
        }
    }
}
