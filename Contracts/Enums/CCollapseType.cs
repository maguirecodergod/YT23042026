namespace HTT.BlazorWasm.App.Contracts;

public enum CCollapseType
{
    [HTTMetadata("Layout.Sidebar.Toggle.Expand")]
    Expanded = 1,
    [HTTMetadata("Layout.Sidebar.Toggle.Collapse")]
    Collapsed = 2,
    [HTTMetadata("Layout.Sidebar.Toggle.Hide")]
    Hidden = 3
}
