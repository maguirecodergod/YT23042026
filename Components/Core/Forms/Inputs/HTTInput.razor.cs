using System.Globalization;
using System.Text;

namespace HTT.BlazorWasm.App.Components
{
    /// <summary>
    /// An enterprise-grade, highly extensible input component.
    /// Supports dynamic mapping of CInputType to HTML elements, generic TValue binding,
    /// and specialized rendering for checkboxes, files, and text inputs.
    /// </summary>
    public partial class HTTInput<TValue> : HTTComponentBase
    {
        /// <summary>
        /// The specific input type, mapping to HTML5 input types.
        /// </summary>
        [Parameter] public CInputType Type { get; set; } = CInputType.Text;

        /// <summary>
        /// The value bound to the input.
        /// </summary>
        [Parameter] public TValue? Value { get; set; }

        /// <summary>
        /// Callback for when the value changes.
        /// </summary>
        [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }

        /// <summary>
        /// Placeholder text (used as label for checkboxes).
        /// </summary>
        [Parameter] public string? Placeholder { get; set; }

        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ReadOnly { get; set; }
        
        /// <summary>
        /// Optional Bootstrap Icon class.
        /// </summary>
        [Parameter] public string? Icon { get; set; }
        
        [Parameter] public CSpacingType Size { get; set; } = CSpacingType.MD;
        
        /// <summary>
        /// If true, shows a clear button when the input has a value.
        /// </summary>
        [Parameter] public bool ShowClearButton { get; set; }
        
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string Placement { get; set; } = "left";
        [Parameter] public Func<DateTime, bool>? IsDateDisabled { get; set; }
        
        // Range support
        [Parameter] public DateTimeOffset? RangeStart { get; set; }
        [Parameter] public EventCallback<DateTimeOffset?> RangeStartChanged { get; set; }
        [Parameter] public DateTimeOffset? RangeEnd { get; set; }
        [Parameter] public EventCallback<DateTimeOffset?> RangeEndChanged { get; set; }
        
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        private ElementReference _inputElement;

        public async Task FocusAsync()
        {
            try
            {
                await _inputElement.FocusAsync();
            }
            catch
            {
                // Fallback for non-text inputs or if JS not ready
            }
        }

        protected bool IsCheckboxOrRadio => Type == CInputType.Checkbox || Type == CInputType.Radio;
        protected bool IsFile => Type == CInputType.File;
        protected bool IsRangeType => Type == CInputType.DateRange || Type == CInputType.DateTimeRange || Type == CInputType.TimeRange;
        protected bool IsDateOrTimePickerType => Type == CInputType.DateOnly || Type == CInputType.DateTime || Type == CInputType.DateTimeLocal || Type == CInputType.DateRange || Type == CInputType.DateTimeRange || Type == CInputType.TimeOnly || Type == CInputType.TimeRange;

        protected string HtmlType => Type switch
        {
            CInputType.Button => "button",
            CInputType.Checkbox => "checkbox",
            CInputType.Color => "color",
            CInputType.DateOnly => "date",
            CInputType.DateTime => "datetime-local",
            CInputType.DateTimeLocal => "datetime-local",
            CInputType.Email => "email",
            CInputType.File => "file",
            CInputType.Hidden => "hidden",
            CInputType.Image => "image",
            CInputType.Month => "month",
            CInputType.Number => "number",
            CInputType.Password => "password",
            CInputType.Radio => "radio",
            CInputType.Reset => "reset",
            CInputType.Search => "search",
            CInputType.Submit => "submit",
            CInputType.Text => "text",
            CInputType.TimeOnly => "time",
            CInputType.Url => "url",
            CInputType.Week => "week",
            _ => "text"
        };

        protected string? FormattedValue => FormatValueAsString(Value);

        protected string CssClass => BuildCssClass();

        private string BuildCssClass()
        {
            var sb = new StringBuilder();
            
            if (IsCheckboxOrRadio)
            {
                sb.Append("htt-input-checkbox-radio");
            }
            else
            {
                sb.Append("htt-input-wrapper");
                sb.Append($" htt-input-wrapper--{Size.ToString().ToLower()}");

                if (Disabled) sb.Append(" htt-input-wrapper--disabled");
                if (ReadOnly) sb.Append(" htt-input-wrapper--readonly");
                if (!string.IsNullOrEmpty(Icon)) sb.Append(" htt-input-wrapper--has-icon");
                if (ShowClearButton && !string.IsNullOrEmpty(FormattedValue) && !Disabled && !ReadOnly)
                    sb.Append(" htt-input-wrapper--has-clear");
            }

            if (!string.IsNullOrWhiteSpace(Class))
                sb.Append($" {Class}");

            return sb.ToString().Trim();
        }

        protected bool GetCheckboxCheckedValue()
        {
            if (Value is bool b) return b;
            return false;
        }

        protected async Task HandleCheckboxChange(ChangeEventArgs e)
        {
            if (e.Value is bool b)
            {
                await UpdateValueAsync((TValue)(object)b);
            }
        }

        protected async Task HandleTextChange(ChangeEventArgs e)
        {
            await ParseAndSetStringValue(e.Value?.ToString());
        }

        protected async Task HandleTextInput(ChangeEventArgs e)
        {
            // Can be used for "on-keypress" immediate updating if a specific Parameter requests it.
        }

        protected async Task HandleFileChange(ChangeEventArgs e)
        {
            // Stub for file upload logic integration
        }

        protected async Task ClearInput()
        {
            await UpdateValueAsync(default);
        }

        private async Task ParseAndSetStringValue(string? valueStr)
        {
            if (TryParseValueFromString(valueStr, out var parsedValue))
            {
                await UpdateValueAsync(parsedValue);
            }
        }

        private async Task UpdateValueAsync(TValue? newValue)
        {
            Value = newValue;
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(newValue);
            }
        }

        // --- Range Types UI Handlers ---
        protected string GetRangeHtmlType()
        {
            return Type switch
            {
                CInputType.DateRange => "date",
                CInputType.TimeRange => "time",
                CInputType.DateTimeRange => "datetime-local",
                _ => "text"
            };
        }

        private string? _rangeStart;
        private string? _rangeEnd;

        protected async Task HandleRangeStartChange(ChangeEventArgs e)
        {
            _rangeStart = e.Value?.ToString();
            await TryCommitRangeValue();
        }

        protected async Task HandleRangeEndChange(ChangeEventArgs e)
        {
            _rangeEnd = e.Value?.ToString();
            await TryCommitRangeValue();
        }

        private async Task TryCommitRangeValue()
        {
            // Fallback for range: join by pipe. E.g. "2023-01-01|2023-12-31"
            // For complex enterprise objects, the wrapper component parses this string back out.
            var combined = $"{_rangeStart}|{_rangeEnd}";
            if (TryParseValueFromString(combined, out var parsedValue))
            {
                await UpdateValueAsync(parsedValue);
            }
        }

        private bool TryParseValueFromString(string? value, out TValue? result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = default;
                return true;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            try
            {
                if (targetType == typeof(string)) { result = (TValue)(object)value; return true; }
                if (targetType == typeof(int)) { result = (TValue)(object)int.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(long)) { result = (TValue)(object)long.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(double)) { result = (TValue)(object)double.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(decimal)) { result = (TValue)(object)decimal.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(bool)) { result = (TValue)(object)bool.Parse(value); return true; }
                if (targetType == typeof(Guid)) { result = (TValue)(object)Guid.Parse(value); return true; }
                if (targetType == typeof(DateTime)) { result = (TValue)(object)DateTime.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(DateTimeOffset)) { result = (TValue)(object)DateTimeOffset.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(DateOnly)) { result = (TValue)(object)DateOnly.Parse(value, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(TimeOnly)) { result = (TValue)(object)TimeOnly.Parse(value, CultureInfo.InvariantCulture); return true; }

                if (targetType.IsEnum)
                {
                    result = (TValue)Enum.Parse(targetType, value, true);
                    return true;
                }

                var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    result = (TValue?)converter.ConvertFromInvariantString(value);
                    return true;
                }
            }
            catch
            {
                // Parsing failed, fallback to default
            }

            result = default;
            return false;
        }

        private string? FormatValueAsString(TValue? value)
        {
            if (value == null) return null;

            return value switch
            {
                string s => s,
                DateTime dt => FormatDateTime(dt),
                DateTimeOffset dto => FormatDateTime(dto.DateTime),
                DateOnly @do => @do.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TimeOnly to => to.ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        private string FormatDateTime(DateTime dt)
        {
            // Align formatting with HTML5 native input constraints
            return Type switch
            {
                CInputType.DateTimeLocal or CInputType.DateTime => dt.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                CInputType.DateOnly => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CInputType.Month => dt.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                CInputType.TimeOnly => dt.ToString("HH:mm", CultureInfo.InvariantCulture),
                _ => dt.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)
            };
        }
    }
}