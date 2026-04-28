namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTDatePicker<TValue> : HTTComponentBase
    {
        [Parameter] public TValue? Value { get; set; }
        [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }

        [Parameter] public CInputType Type { get; set; } = CInputType.DateOnly;

        [Parameter] public string? Placeholder { get; set; }
        [Parameter] public string Icon { get; set; } = "bi-calendar3";
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ReadOnly { get; set; }
        [Parameter] public bool ShowClearButton { get; set; } = true;
        [Parameter] public string Placement { get; set; } = "left";
        [Parameter] public Func<DateTime, bool>? IsDateDisabled { get; set; }

        // Range support for separate properties
        [Parameter] public DateTimeOffset? RangeStart { get; set; }
        [Parameter] public EventCallback<DateTimeOffset?> RangeStartChanged { get; set; }
        [Parameter] public DateTimeOffset? RangeEnd { get; set; }
        [Parameter] public EventCallback<DateTimeOffset?> RangeEndChanged { get; set; }

        [Parameter] public string Class { get; set; } = string.Empty;
        protected string CssClass => $"{(Disabled ? "is-disabled" : "")} {(ReadOnly ? "is-readonly" : "")} {Class}".Trim();

        [Parameter] public string? ValueString { get; set; }

        private bool IsRange => Type == CInputType.DateRange || Type == CInputType.DateTimeRange || Type == CInputType.TimeRange;
        private bool IsTime => Type == CInputType.TimeOnly || Type == CInputType.TimeRange || Type == CInputType.DateTime || Type == CInputType.DateTimeLocal || Type == CInputType.DateTimeRange;
        private bool IsDate => Type == CInputType.DateOnly || Type == CInputType.DateRange || Type == CInputType.DateTime || Type == CInputType.DateTimeLocal || Type == CInputType.DateTimeRange;

        private bool _isOpen = false;
        private ElementReference _pickerRef;

        // Calendar State
        private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // Selection State
        private DateTime? _selectedStartDate;
        private DateTime? _selectedEndDate;
        private DateTime? _hoverDate;

        private enum PickerView { Days, Months, Years }
        private PickerView _currentView = PickerView.Days;
        private int _yearPageStart;

        // Time State
        private int _startHour = 9;
        private int _startMinute = 0;
        private bool _startIsPM = false;

        private int _endHour = 17;
        private int _endMinute = 30;
        private bool _endIsPM = true;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            // Parse initial value
            ParseInitialValue();
        }

        private void ParseInitialValue()
        {
            if (Value == null && RangeStart == null && RangeEnd == null)
            {
                _selectedStartDate = null;
                _selectedEndDate = null;
                return;
            }

            // Prioritize RangeStart/RangeEnd if they are set
            if (IsRange && (RangeStart != null || RangeEnd != null))
            {
                if (RangeStart.HasValue) _selectedStartDate = RangeStart.Value.DateTime;
                if (RangeEnd.HasValue) _selectedEndDate = RangeEnd.Value.DateTime;

                if (_selectedStartDate != null) _currentMonth = new DateTime(_selectedStartDate.Value.Year, _selectedStartDate.Value.Month, 1);
                return;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            if (Value is DateTime dt)
            {
                _selectedStartDate = dt;
                _currentMonth = new DateTime(dt.Year, dt.Month, 1);
                SyncTimeFromValue(dt, true);
            }
            else if (Value is DateTimeOffset dto)
            {
                _selectedStartDate = dto.DateTime;
                _currentMonth = new DateTime(dto.Year, dto.Month, 1);
                SyncTimeFromValue(dto.DateTime, true);
            }
            else if (Value is DateOnly @do)
            {
                _selectedStartDate = @do.ToDateTime(TimeOnly.MinValue);
                _currentMonth = new DateTime(@do.Year, @do.Month, 1);
            }
            else if (Value is TimeOnly @to)
            {
                _selectedStartDate = DateTime.Today.Add(@to.ToTimeSpan());
                SyncTimeFromValue(_selectedStartDate.Value, true);
            }
            else if (Value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (IsRange && s.Contains('|'))
                {
                    var parts = s.Split('|');
                    if (DateTime.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDt))
                    {
                        _selectedStartDate = startDt;
                        _currentMonth = new DateTime(startDt.Year, startDt.Month, 1);
                        SyncTimeFromValue(startDt, true);
                    }
                    if (DateTime.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime endDt))
                    {
                        _selectedEndDate = endDt;
                        SyncTimeFromValue(endDt, false);
                    }
                }
                else if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDt))
                {
                    _selectedStartDate = parsedDt;
                    _currentMonth = new DateTime(parsedDt.Year, parsedDt.Month, 1);
                    SyncTimeFromValue(parsedDt, true);
                }
            }
        }

        private void SyncTimeFromValue(DateTime dt, bool isStart)
        {
            int hour = dt.Hour;
            bool isPm = hour >= 12;
            if (hour > 12) hour -= 12;
            if (hour == 0) hour = 12;

            if (isStart)
            {
                _startHour = hour;
                _startMinute = dt.Minute;
                _startIsPM = isPm;
            }
            else
            {
                _endHour = hour;
                _endMinute = dt.Minute;
                _endIsPM = isPm;
            }
        }

        private async Task ToggleDropdown()
        {
            if (Disabled || ReadOnly) return;
            _isOpen = !_isOpen;
            if (_isOpen && _selectedStartDate == null)
            {
                _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }
        }

        private async Task CloseDropdown()
        {
            _isOpen = false;
            _currentView = PickerView.Days;
        }

        private void ShowMonthView()
        {
            _currentView = PickerView.Months;
        }

        private void ShowYearView()
        {
            _currentView = PickerView.Years;
            _yearPageStart = (_currentMonth.Year / 12) * 12;
        }

        private void SelectMonth(int month)
        {
            _currentMonth = new DateTime(_currentMonth.Year, month, 1);
            _currentView = PickerView.Days;
        }

        private void SelectYear(int year)
        {
            _currentMonth = new DateTime(year, _currentMonth.Month, 1);
            _currentView = PickerView.Months;
        }

        private void PreviousYearPage()
        {
            _yearPageStart -= 12;
        }

        private void NextYearPage()
        {
            _yearPageStart += 12;
        }

        private void PreviousMonth()
        {
            _currentMonth = _currentMonth.AddMonths(-1);
        }

        private void NextMonth()
        {
            _currentMonth = _currentMonth.AddMonths(1);
        }
        private async Task ApplyRange()
        {
            await CommitValue();
            _isOpen = false;
        }

        private async Task CommitValue()
        {
            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            if (_selectedStartDate == null && IsDate) return;

            // If it's time only, and date is null, use today
            if (_selectedStartDate == null && IsTime) _selectedStartDate = DateTime.Today;

            // Apply time part if visible
            if (IsTime && _selectedStartDate != null)
            {
                _selectedStartDate = CombineDateAndTime(_selectedStartDate.Value, _startHour, _startMinute, _startIsPM);

                if (IsRange && _selectedEndDate != null)
                {
                    _selectedEndDate = CombineDateAndTime(_selectedEndDate.Value, _endHour, _endMinute, _endIsPM);
                }
            }

            if (_selectedStartDate == null) return;

            object? newValue = null;

            if (targetType == typeof(DateTime))
            {
                newValue = _selectedStartDate.Value;
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                newValue = new DateTimeOffset(_selectedStartDate.Value);
            }
            else if (targetType == typeof(DateOnly))
            {
                newValue = DateOnly.FromDateTime(_selectedStartDate.Value);
            }
            else if (targetType == typeof(TimeOnly))
            {
                newValue = TimeOnly.FromDateTime(_selectedStartDate.Value);
            }
            else if (targetType == typeof(string))
            {
                if (IsRange && _selectedEndDate != null)
                {
                    string format = "yyyy-MM-dd";
                    if (IsDate && IsTime) format = "yyyy-MM-dd HH:mm";
                    else if (!IsDate && IsTime) format = "HH:mm";

                    newValue = $"{_selectedStartDate.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}|{_selectedEndDate.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}";
                }
                else
                {
                    string format = "yyyy-MM-dd";
                    if (IsDate && IsTime) format = "yyyy-MM-dd HH:mm";
                    else if (!IsDate && IsTime) format = "HH:mm";

                    newValue = _selectedStartDate.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            if (newValue != null)
            {
                Value = (TValue)newValue;
                await ValueChanged.InvokeAsync(Value);
            }

            // Update RangeStart/RangeEnd if they are being used
            if (IsRange && _selectedStartDate != null && _selectedEndDate != null)
            {
                if (RangeStartChanged.HasDelegate)
                {
                    await RangeStartChanged.InvokeAsync(new DateTimeOffset(_selectedStartDate.Value));
                }
                if (RangeEndChanged.HasDelegate)
                {
                    await RangeEndChanged.InvokeAsync(new DateTimeOffset(_selectedEndDate.Value));
                }
            }
        }

        private object? ConvertToTargetType(DateTime dt, Type targetType)
        {
            var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (actualType == typeof(DateTime)) return dt;
            if (actualType == typeof(DateTimeOffset)) return new DateTimeOffset(dt);
            if (actualType == typeof(DateOnly)) return DateOnly.FromDateTime(dt);
            if (actualType == typeof(TimeOnly)) return TimeOnly.FromDateTime(dt);
            if (actualType == typeof(string)) return dt.ToString("yyyy-MM-dd HH:mm");
            return dt;
        }

        private DateTime CombineDateAndTime(DateTime date, int hour, int minute, bool isPM)
        {
            int h = hour;
            if (isPM && h < 12) h += 12;
            if (!isPM && h == 12) h = 0;
            return new DateTime(date.Year, date.Month, date.Day, h, minute, 0);
        }

        private async Task SelectDate(DateTime date)
        {
            if (IsDateDisabled != null && IsDateDisabled(date)) return;

            if (!IsRange)
            {
                _selectedStartDate = date;
                await CommitValue();
                _isOpen = false;
                return;
            }

            // Range Logic
            if (_selectedStartDate == null || (_selectedStartDate != null && _selectedEndDate != null))
            {
                // Start new range
                _selectedStartDate = date;
                _selectedEndDate = null;
            }
            else
            {
                // Complete range
                if (date < _selectedStartDate)
                {
                    _selectedEndDate = _selectedStartDate;
                    _selectedStartDate = date;
                }
                else
                {
                    _selectedEndDate = date;
                }
            }
        }

        private void HandleMouseOver(DateTime date)
        {
            if (IsRange && _selectedStartDate != null && _selectedEndDate == null)
            {
                _hoverDate = date;
            }
        }

        private async Task ClearSelection()
        {
            _selectedStartDate = null;
            _selectedEndDate = null;
            _hoverDate = null;
            Value = default;
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(Value);
            }
            _isOpen = false;
        }


        private string FormatDate(DateTime dt)
        {
            if (IsDate && IsTime) return dt.ToString("MMM dd, yyyy hh:mm tt", L.CurrentCulture);
            if (!IsDate && IsTime) return dt.ToString("hh:mm tt", L.CurrentCulture);
            return dt.ToString("MMM dd, yyyy", L.CurrentCulture);
        }

        private string GetDisplayValue()
        {
            if (_selectedStartDate == null) return string.Empty;

            if (IsRange)
            {
                if (_selectedEndDate != null)
                    return $"{FormatDate(_selectedStartDate.Value)} - {FormatDate(_selectedEndDate.Value)}";
                return $"{FormatDate(_selectedStartDate.Value)} - ...";
            }

            return FormatDate(_selectedStartDate.Value);
        }

        // Calendar Generation
        private List<CalendarDay> GetDaysInMonth()
        {
            var days = new List<CalendarDay>();
            var firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

            // Previous month padding
            int prevMonthDays = (int)firstDayOfMonth.DayOfWeek;
            var prevMonth = _currentMonth.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            for (int i = prevMonthDays - 1; i >= 0; i--)
            {
                days.Add(new CalendarDay
                {
                    Date = new DateTime(prevMonth.Year, prevMonth.Month, daysInPrevMonth - i),
                    IsCurrentMonth = false
                });
            }

            // Current month days
            for (int i = 1; i <= daysInMonth; i++)
            {
                days.Add(new CalendarDay
                {
                    Date = new DateTime(_currentMonth.Year, _currentMonth.Month, i),
                    IsCurrentMonth = true
                });
            }

            // Next month padding to fill 42 cells (6 rows)
            int remainingCells = 42 - days.Count;
            var nextMonth = _currentMonth.AddMonths(1);
            for (int i = 1; i <= remainingCells; i++)
            {
                days.Add(new CalendarDay
                {
                    Date = new DateTime(nextMonth.Year, nextMonth.Month, i),
                    IsCurrentMonth = false
                });
            }

            return days;
        }

        private bool IsSelected(DateTime date)
        {
            return date == _selectedStartDate || date == _selectedEndDate;
        }

        private bool IsInRange(DateTime date)
        {
            if (!IsRange || _selectedStartDate == null) return false;

            if (_selectedEndDate != null)
            {
                return date > _selectedStartDate && date < _selectedEndDate;
            }

            if (_hoverDate != null)
            {
                var start = _selectedStartDate < _hoverDate ? _selectedStartDate : _hoverDate;
                var end = _selectedStartDate > _hoverDate ? _selectedStartDate : _hoverDate;
                return date > start && date < end;
            }

            return false;
        }

        private bool IsRangeStart(DateTime date)
        {
            if (!IsRange) return false;
            if (_selectedEndDate != null) return date == _selectedStartDate;
            if (_hoverDate != null) return date == (_selectedStartDate < _hoverDate ? _selectedStartDate : _hoverDate);
            return date == _selectedStartDate;
        }

        private bool IsRangeEnd(DateTime date)
        {
            if (!IsRange) return false;
            if (_selectedEndDate != null) return date == _selectedEndDate;
            if (_hoverDate != null) return date == (_selectedStartDate > _hoverDate ? _selectedStartDate : _hoverDate);
            return false;
        }

        private void AdjustHour(ref int hour, int delta)
        {
            hour += delta;
            if (hour > 12) hour = 1;
            if (hour < 1) hour = 12;
        }

        private void AdjustMinute(ref int minute, int delta)
        {
            minute += delta;
            if (minute >= 60) minute = 0;
            if (minute < 0) minute = 59;
        }

        public class CalendarDay
        {
            public DateTime Date { get; set; }
            public bool IsCurrentMonth { get; set; }
        }
    }
}
