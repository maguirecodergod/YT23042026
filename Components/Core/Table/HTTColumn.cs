using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace HTT.BlazorWasm.App.Components
{
    public enum CColumnAlign
    {
        Left,
        Center,
        Right
    }

    public enum CColumnFixedPosition
    {
        None,
        Left,
        Right
    }

    public partial class HTTColumn<TItem>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        
        [Parameter] public CColumnFixedPosition Fixed { get; set; } = CColumnFixedPosition.None;
        
        [Parameter] public RenderFragment<TItem>? Template 
        { 
            get => CellTemplate; 
            set => CellTemplate = value; 
        }
        
        [Parameter] public CColumnAlign Align { get; set; } = CColumnAlign.Left;
        
        [Parameter] public string? Format { get; set; }
        
        // Caching the compiled field to avoid recompiling it for every row/cell
        private Func<TItem, object>? _compiledField;
        internal Func<TItem, object>? CompiledField
        {
            get
            {
                if (_compiledField == null && Field != null)
                {
                    _compiledField = Field.Compile();
                }
                return _compiledField;
            }
        }
    }
}
