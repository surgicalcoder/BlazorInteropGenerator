using System.Collections.Generic;

namespace GoLive.Generator.BlazorInterop
{
    public class JavascriptItem
    {
        public JavascriptItem()
        {
            Params = new();
        }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> Params { get; set; }
    }
}

