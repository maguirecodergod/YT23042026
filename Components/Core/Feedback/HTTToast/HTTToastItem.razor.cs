using System.Diagnostics;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTToastItem
    {
        [Parameter] public ToastModel Toast { get; set; } = default!;

        private Stopwatch _stopwatch = new();
        private CancellationTokenSource? _cts;
        private bool _isPaused;
        private double _remainingTime;

        protected override void OnInitialized()
        {
            _remainingTime = Toast.Duration;
            StartTimer();
        }

        private void StartTimer()
        {
            if (Toast.Duration <= 0) return;

            _stopwatch.Start();
            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    while (_remainingTime > 0 && !_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(100, _cts.Token);
                        
                        if (!_isPaused)
                        {
                            _remainingTime -= _stopwatch.ElapsedMilliseconds;
                            _stopwatch.Restart();
                            
                            if (_remainingTime <= 0)
                            {
                                await InvokeAsync(Close);
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { }
            });
        }

        private void PauseTimer()
        {
            _isPaused = true;
            _stopwatch.Stop();
            StateHasChanged();
        }

        private void ResumeTimer()
        {
            _isPaused = false;
            _stopwatch.Restart();
            StateHasChanged();
        }

        private async Task Close()
        {
            _cts?.Cancel();
            Toast.IsClosing = true;
            await InvokeAsync(StateHasChanged);

            await Task.Delay(400);
            ToastService.Remove(Toast.Id);
        }

        private void HandleAction()
        {
            Toast.OnActionClick?.Invoke();
            _ = Close();
        }

        private string GetToastClass()
        {
            var classes = $"htt-toast-{Toast.Type.ToString().ToLower()}";
            if (Toast.IsClosing) classes += " htt-toast-closing";
            if (ToastService.GlobalOptions.EnableGlassmorphism) classes += " htt-toast-glass";
            return classes;
        }

        private string GetDefaultIcon()
        {
            return Toast.Type switch
            {
                CToastType.Success => "bi bi-check-circle-fill",
                CToastType.Error => "bi bi-x-circle-fill",
                CToastType.Warning => "bi bi-exclamation-triangle-fill",
                CToastType.Info => "bi bi-info-circle-fill",
                _ => "bi bi-bell-fill"
            };
        }

        private string GetTimeAgo()
        {
            var diff = DateTime.Now - Toast.CreatedAt;
            if (diff.TotalSeconds < 60) return "Just now";
            return $"{(int)diff.TotalMinutes}m ago";
        }

        public override void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}
