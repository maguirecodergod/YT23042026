using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTContainer : HTTComponentBase
    {
        #region Parameters
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? Id { get; set; }
        [Parameter] public string? Role { get; set; }

        [Parameter] public bool Fluid { get; set; } = false;
        [Parameter] public string MaxWidth { get; set; } = "1200px";

        [Parameter] public CSpacingType Padding { get; set; } = CSpacingType.MD;
        [Parameter] public CSpacingType Margin { get; set; } = CSpacingType.None;

        [Parameter] public Dictionary<string, string>? MaxWidthBreakpoints { get; set; }
        
        /// <summary>
        /// Gets or sets the theme mode for the container.
        /// System: Always follows the global IHTTThemeService state, ignoring parent overrides.
        /// Light/Dark: Explicitly overrides the theme for this container and its children.
        /// </summary>
        [Parameter] public new CThemeType Theme { get; set; } = CThemeType.System;
        #endregion

        protected string CssClass => BuildCssClass();
        protected string CssStyle => BuildCssStyle();

        /// <summary>
        /// Resolves the effective theme. 
        /// In System mode, it explicitly pulls from the IHTTThemeService to ensure 
        /// it stays in sync with the global theme even if parent containers have overrides.
        /// </summary>
        protected string EffectiveTheme => Theme switch
        {
            CThemeType.Light => "light",
            CThemeType.Dark => "dark",
            _ => base.Theme.IsDark ? "dark" : "light"
        };

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            sb.Append("htt-container");
            
            if (Fluid) sb.Append(" htt-container-fluid");
            
            // Add theme class for styling flexibility if needed
            sb.Append($" htt-theme-{EffectiveTheme}");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        private string BuildCssStyle()
        {
            var styles = new List<string>();

            // Spacing tokens from theme.css
            styles.Add($"--htt-padding: {GetSpacingValue(Padding)}");
            styles.Add($"--htt-margin: {GetSpacingValue(Margin)}");

            // Layout logic
            if (!Fluid)
            {
                styles.Add($"--htt-max-width: {MaxWidth}");
                styles.AddRange(BuildBreakpointStyles());
            }

            if (!string.IsNullOrWhiteSpace(Style))
                styles.Add(Style);

            return string.Join("; ", styles);
        }

        private string GetSpacingValue(CSpacingType spacing)
        {
            return spacing switch
            {
                CSpacingType.None => "var(--space-0)",
                CSpacingType.XS => "var(--space-1)",
                CSpacingType.SM => "var(--space-2)",
                CSpacingType.MD => "var(--space-4)",
                CSpacingType.LG => "var(--space-6)",
                CSpacingType.XL => "var(--space-8)",
                _ => "var(--space-4)"
            };
        }

        private IEnumerable<string> BuildBreakpointStyles()
        {
            if (MaxWidthBreakpoints == null) yield break;

            foreach (var bp in MaxWidthBreakpoints)
            {
                var key = bp.Key.ToUpper();
                yield return $"--htt-max-width-{key.ToLower()}: {bp.Value}";
            }
        }
    }
}
