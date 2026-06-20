using System;
using System.Text;

namespace GoLive.Generator.BlazorInterop;

public class SourceStringBuilder
{
    private readonly string SingleIndent = new string(' ', 4);

    private int _indent = 0;
    private readonly StringBuilder _stringBuilder;

    public SourceStringBuilder()
    {
        _stringBuilder = new StringBuilder();
    }

    public void IncreaseIndent()
    {
        _indent++;
    }

    public void DecreaseIndent()
    {
        _indent--;
    }

    public void AppendOpenCurlyBracketLine()
    {
        AppendLine("{");
        IncreaseIndent();
    }

    public void AppendCloseCurlyBracketLine()
    {
        DecreaseIndent();
        AppendLine("}");
    }

    public void Append(string text)
    {
        for (int i = 0; i < _indent; i++)
        {
            _stringBuilder.Append(SingleIndent);
        }

        _stringBuilder.Append(text);
    }

    public void AppendLine()
    {
        _stringBuilder.Append(Environment.NewLine);
    }

    public void AppendLine(string text)
    {
        Append(text);
        AppendLine();
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }
}