using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTStack : HTTComponentBase
    {
        #region Parameters
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? Id { get; set; }
        [Parameter] public string? Role { get; set; }

        // Layout (Flex)
        [Parameter] public CDirectionType Direction { get; set; } = CDirectionType.Column;
        [Parameter] public CAlignType Align { get; set; } = CAlignType.Stretch;
        [Parameter] public CJustifyType Justify { get; set; } = CJustifyType.Start;
        [Parameter] public bool Wrap { get; set; } = false;

        // Spacing System
        [Parameter] public CSpacingType Spacing { get; set; } = CSpacingType.None;

        // Responsive System
        [Parameter] public Dictionary<string, CDirectionType>? DirectionBreakpoints { get; set; }
        [Parameter] public Dictionary<string, CSpacingType>? SpacingBreakpoints { get; set; }

        // Theme Support
        [Parameter] public new CThemeType Theme { get; set; } = CThemeType.System;
        #endregion

        protected string CssClass => BuildCssClass();
        protected string CssStyle => BuildCssStyle();

        /// <summary>
        /// Resolves the effective theme based on parameters or global service.
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
            sb.Append("htt-stack");
            
            // Add theme class for styling flexibility
            sb.Append($" htt-theme-{EffectiveTheme}");

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        private string BuildCssStyle()
        {
            var styles = new List<string>();

            // Base Layout Styles
            styles.Add($"--htt-stack-direction: {Direction.ToString().ToLower()}");
            styles.Add($"--htt-stack-align: {MapAlign(Align)}");
            styles.Add($"--htt-stack-justify: {MapJustify(Justify)}");
            styles.Add($"--htt-stack-wrap: {(Wrap ? "wrap" : "nowrap")}");
            styles.Add($"--htt-stack-spacing: {GetSpacingValue(Spacing)}");

            // Responsive Styles
            styles.AddRange(BuildResponsiveStyles());

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
                _ => "var(--space-0)"
            };
        }

        private string MapAlign(CAlignType align)
        {
            return align switch
            {
                CAlignType.Start => "flex-start",
                CAlignType.Center => "center",
                CAlignType.End => "flex-end",
                CAlignType.Stretch => "stretch",
                CAlignType.Baseline => "baseline",
                _ => "stretch"
            };
        }

        private string MapJustify(CJustifyType justify)
        {
            return justify switch
            {
                CJustifyType.Start => "flex-start",
                CJustifyType.Center => "center",
                CJustifyType.End => "flex-end",
                CJustifyType.Between => "space-between",
                CJustifyType.Around => "space-around",
                CJustifyType.Evenly => "space-evenly",
                _ => "flex-start"
            };
        }

        private IEnumerable<string> BuildResponsiveStyles()
        {
            var styles = new List<string>();

            if (DirectionBreakpoints != null)
            {
                foreach (var bp in DirectionBreakpoints)
                {
                    styles.Add($"--htt-stack-direction-{bp.Key.ToLower()}: {bp.Value.ToString().ToLower()}");
                }
            }

            if (SpacingBreakpoints != null)
            {
                foreach (var bp in SpacingBreakpoints)
                {
                    styles.Add($"--htt-stack-spacing-{bp.Key.ToLower()}: {GetSpacingValue(bp.Value)}");
                }
            }

            return styles;
        }
    }
}
