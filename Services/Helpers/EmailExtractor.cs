using System.Text.RegularExpressions;
using System.Linq;

namespace WebParser.Services;

public static partial class EmailExtractor
{
    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")]
    private static partial Regex EmailRegex();

    public static List<string> ExtractEmails(string text)
    {
        var matches = EmailRegex().Matches(text);
        return matches.Select(m => m.Value).ToList();
    }
}