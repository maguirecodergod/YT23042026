namespace HTT.BlazorWasm.App.Components
{
    public class HTTComponentBase : ComponentBase
    {
        [Inject] protected JSRuntime JS { get; set; } = default!;
        [Inject] ILogger<HTTComponentBase> Logger { get; set; } = default!;
    }
}