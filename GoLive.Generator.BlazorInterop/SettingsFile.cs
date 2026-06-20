using System;
using System.Linq;

namespace GoLive.Generator.BlazorInterop;

public class SettingsFile : IEquatable<SettingsFile>
{
    public string Output { get; set; }
    public string ClassName { get; set; }
    public string Source { get; set; }
    public string Namespace { get; set; }
    public string ObjectToInterop { get; set; }
    public string SourceContents { get; set; }
    public string[] Init { get; set; }
    public bool Equals(SettingsFile other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Output == other.Output
            && ClassName == other.ClassName
            && Source == other.Source
            && Namespace == other.Namespace
            && ObjectToInterop == other.ObjectToInterop
            && SourceContents == other.SourceContents
            && (Init == null && other.Init == null || Init != null && other.Init != null && Init.SequenceEqual(other.Init));
    }

    public override bool Equals(object obj) => Equals(obj as SettingsFile);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (Output?.GetHashCode() ?? 0);
            hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
            hash = hash * 31 + (Source?.GetHashCode() ?? 0);
            hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
            hash = hash * 31 + (ObjectToInterop?.GetHashCode() ?? 0);
            hash = hash * 31 + (SourceContents?.GetHashCode() ?? 0);
            return hash;
        }
    }

}
