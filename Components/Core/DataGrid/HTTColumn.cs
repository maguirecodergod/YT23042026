using System.Linq.Expressions;

namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTColumn<TItem> : ComponentBase
    {
        [CascadingParameter] public HTTGrid<TItem>? Grid { get; set; }
        [CascadingParameter] public HTTTable<TItem>? Table { get; set; }

        [Parameter] public string? Title { get; set; }
        [Parameter] public Expression<Func<TItem, object>>? Field { get; set; }
        [Parameter] public string? FieldName { get; set; }
        [Parameter] public string? Width { get; set; }
        [Parameter] public bool Sortable { get; set; } = true;
        [Parameter] public bool Filterable { get; set; } = true;
        [Parameter] public bool Resizable { get; set; } = true;
        [Parameter] public bool Visible { get; set; } = true;
        [Parameter] public RenderFragment<TItem>? CellTemplate { get; set; }
        [Parameter] public RenderFragment? HeaderTemplate { get; set; }

        protected override void OnInitialized()
        {
            if (Grid != null)
            {
                Grid.AddColumn(this);
            }
            else if (Table != null)
            {
                Table.AddColumn(this);
            }
            else
            {
                // Fallback or warning if needed, but some use cases might not need registration immediately
            }
        }

        public string? GetFieldName()
        {
            if (!string.IsNullOrEmpty(FieldName)) return FieldName;
            if (Field == null) return null;

            if (Field.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (Field.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExp)
            {
                return memberExp.Member.Name;
            }

            return null;
        }

        public object? GetValue(TItem item)
        {
            if (Field == null) return null;
            var compiled = Field.Compile();
            return compiled(item);
        }
    }
}
