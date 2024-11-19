namespace GoLive.Generator.BlazorInterop;

public class SettingsFile
{
    public string Output { get; set; }
    public string ClassName { get; set; }
    public string Source { get; set; }
    public string Namespace { get; set; }
    public string ObjectToInterop { get; set; }
    public string SourceContents { get; set; }
    public string[] Init { get; set; }
}