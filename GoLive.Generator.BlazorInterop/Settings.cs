using System;
using System.Collections.Generic;

namespace GoLive.Generator.BlazorInterop
{
    public class Settings
    {
        public string JavascriptFile { get; set; }

        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string OutputToFile { get; set; }
        
        public string InvokeVoidString { get; set; }
        
        public string InvokeString { get; set; }
        
        public List<String> JSInit { get; set; }
        
        public string MainJsObject { get; set; }
    }
}