using HTT.BlazorWasm.App.Contracts;
using Microsoft.AspNetCore.Components;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTHeading : HTTComponentBase
    {   
        [Parameter] public CHeadingType Type { get; set; } = CHeadingType.H1;
    }
}