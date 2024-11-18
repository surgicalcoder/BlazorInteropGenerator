using System;
using System.Collections.Generic;

namespace GoLive.Generator.BlazorInterop;

public class Settings
{
    public List<SettingsFile> Files { get; set; }
        
    public string InvokeVoidString { get; set; }
        
    public string InvokeString { get; set; }
}