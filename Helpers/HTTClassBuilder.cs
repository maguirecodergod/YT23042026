using System.Text;

namespace HTT.BlazorWasm.App.Helpers
{
    public class HTTClassBuilder
    {
        private readonly StringBuilder _stringBuilder = new();

        public HTTClassBuilder(string? baseClass = null)
        {
            if (!string.IsNullOrWhiteSpace(baseClass))
            {
                _stringBuilder.Append(baseClass);
            }
        }

        public HTTClassBuilder AddClass(string? className, bool condition = true)
        {
            if (condition && !string.IsNullOrWhiteSpace(className))
            {
                if (_stringBuilder.Length > 0)
                {
                    _stringBuilder.Append(' ');
                }
                _stringBuilder.Append(className);
            }
            return this;
        }

        public string Build() => _stringBuilder.ToString().Trim();

        public override string ToString() => Build();
    }
}
