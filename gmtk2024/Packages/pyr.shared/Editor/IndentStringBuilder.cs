using System;
using System.Text;

namespace pyr.Shared.Editor;

public class IndentStringBuilder
{
    public enum Identation
    {
        Tab,
        Space
    }

    private static readonly string[] s_LineSeparators = { "\r\n", "\n", "\r" };

    private readonly StringBuilder _Builder = new();

    private Identation _Identation;
    private bool _IsNewLine = true;

    public IndentStringBuilder(int indent = 4, Identation identation = Identation.Space)
    {
        _Identation = identation;
        IndentLength = indent;
    }

    public int IndentDepth { get; private set; }

    public int IndentLength { get; set; } = 4;

    public static IndentStringBuilder Space(int indent = 4)
    {
        return new IndentStringBuilder(indent);
    }

    public static IndentStringBuilder Tab(int indent = 1)
    {
        return new IndentStringBuilder(indent, Identation.Tab);
    }

    public IndentStringBuilder Indent(int depth = 1)
    {
        IndentDepth += depth;
        return this;
    }

    public IndentStringBuilder Unindent(int depth = 1)
    {
        IndentDepth -= depth;
        return this;
    }

    public IndentStringBuilder ClearIndent()
    {
        IndentDepth = 0;
        return this;
    }

    public IndentStringBuilder Append(string value)
    {
        var lines = value.Split(s_LineSeparators, StringSplitOptions.None);
        for (var i = 0; i < lines.Length; ++i)
        {
            AppendInner(lines[i]);
            if (i < lines.Length - 1) AppendLine();
        }

        return this;
    }

    public IndentStringBuilder AppendFormat(string format, params object[] args)
    {
        Append(string.Format(format, args));
        return this;
    }

    public IndentStringBuilder AppendLineFormat(string format, params object[] args)
    {
        AppendLine(string.Format(format, args));
        return this;
    }

    public IndentStringBuilder AppendLine()
    {
        _Builder.AppendLine();
        _IsNewLine = true;
        return this;
    }

    public IndentStringBuilder AppendLine(string value)
    {
        AppendInner(value);
        AppendLine();
        return this;
    }

    private IndentStringBuilder AppendInner(string value)
    {
        if (_IsNewLine)
        {
            _Builder.Append(IndentString());
            _IsNewLine = false;
        }

        _Builder.Append(value);
        return this;
    }

    private string IndentString()
    {
        return _Identation switch
        {
            Identation.Tab => new string('\t', IndentDepth * IndentLength),
            Identation.Space => new string(' ', IndentDepth * IndentLength),
            _ => throw new ArgumentOutOfRangeException(nameof(_Identation), _Identation, null)
        };
    }

    public override string ToString()
    {
        return _Builder.ToString();
    }
}
