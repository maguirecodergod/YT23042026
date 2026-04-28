using System.Collections.Generic;
using System.Linq;

namespace HTT.BlazorWasm.App.Components
{
    public static class HTTTimeConstants
    {
        public static readonly List<int> Hours = Enumerable.Range(1, 12).ToList();
        public static readonly List<int> Minutes = Enumerable.Range(0, 60).ToList();
        
        public static readonly List<AmPmOption> AmPmOptions = new() 
        { 
            new AmPmOption { Value = false, Label = "AM" }, 
            new AmPmOption { Value = true, Label = "PM" } 
        };

        public class AmPmOption 
        { 
            public bool Value { get; set; } 
            public string Label { get; set; } = string.Empty;
        }
    }
}
