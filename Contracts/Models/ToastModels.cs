using System;

namespace HTT.BlazorWasm.App.Contracts
{
    public class ToastModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public CToastType Type { get; set; } = CToastType.Info;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Duration { get; set; } = 5000;
        public bool IsClosable { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;
        public Action? OnActionClick { get; set; }
        public string? ActionText { get; set; }
        public CToastPosition Position { get; set; } = CToastPosition.TopRight;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int Priority { get; set; } = 0;
        public int Count { get; set; } = 1;
        
        // Internal state
        internal bool IsClosing { get; set; }
    }

    public class ToastOptions
    {
        public int MaxToasts { get; set; } = 5;
        public int DefaultDuration { get; set; } = 5000;
        public CToastPosition Position { get; set; } = CToastPosition.TopRight;
        public bool PreventDuplicates { get; set; } = false;
        public bool NewestOnTop { get; set; } = false;
        public bool EnableGlassmorphism { get; set; } = false;
    }
}
