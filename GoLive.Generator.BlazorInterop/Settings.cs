using System;
using System.Collections.Generic;
using System.Linq;

namespace GoLive.Generator.BlazorInterop;

public class Settings : IEquatable<Settings>
{
    public List<SettingsFile> Files { get; set; }

    public string InvokeVoidString { get; set; }

    public string InvokeString { get; set; }

    public bool Equals(Settings other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return InvokeVoidString == other.InvokeVoidString
            && InvokeString == other.InvokeString
            && (Files == null && other.Files == null
                || Files != null && other.Files != null && Files.SequenceEqual(other.Files));
    }

    public override bool Equals(object obj) => Equals(obj as Settings);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (InvokeVoidString?.GetHashCode() ?? 0);
            hash = hash * 31 + (InvokeString?.GetHashCode() ?? 0);
            return hash;
        }
    }
}