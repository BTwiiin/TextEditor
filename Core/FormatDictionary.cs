namespace TextEditor.Core;

public class FormatDictionary
{
    private Dictionary<Range, List<string>> formats;

    public FormatDictionary()
    {
        formats = new Dictionary<Range, List<string>>();
    }

    public void AddFormatting(Range range, string format)
    {
        if (!formats.ContainsKey(range))
        {
            formats[range] = new List<string>();
        }

        formats[range].Add(format);
    }

    public void RemoveFormatting(Range range)
    {
        if (formats.ContainsKey(range))
        {
            formats.Remove(range);
        }
    }

    public List<string> GetFormatting(Range range)
    {
        if (formats.ContainsKey(range))
        {
            return formats[range];
        }

        return new List<string>();
    }

    public void Clear()
    {
        formats.Clear();
    }
}