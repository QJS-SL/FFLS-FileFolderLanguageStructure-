using System.Text.RegularExpressions;

namespace DSL.Interpreter;

public static class PatternMatcher
{
    public static bool IsMatch(string input, string pattern)
    {
        var regex = ConvertPatternToRegex(pattern);
        return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
    }

    private static string ConvertPatternToRegex(string pattern)
    {
        // 1) Handle custom <L and >L first
        pattern = pattern.Replace("<L", "[a-l]");
        pattern = pattern.Replace(">L", "[l-z]");

        // 2) Handle "contains" syntax: !text  => *text*
        if (pattern.StartsWith("!"))
        {
            var inner = pattern.Substring(1);
            pattern = $"*{inner}*";
        }

        // 3) Escape regex special chars except our glob tokens *, ?, [], -
        string escaped = "";
        foreach (char c in pattern)
        {
            if ("^$.|+(){}".Contains(c))
                escaped += "\\" + c;
            else
                escaped += c;
        }

        // 4) Convert glob tokens to regex
        // *  -> .*
        // ?  -> .
        // [x-y] stays as-is
        string regex = "";
        bool inBracket = false;

        for (int i = 0; i < escaped.Length; i++)
        {
            char c = escaped[i];

            if (c == '[')
            {
                inBracket = true;
                regex += c;
            }
            else if (c == ']')
            {
                inBracket = false;
                regex += c;
            }
            else if (!inBracket && c == '*')
            {
                regex += ".*";
            }
            else if (!inBracket && c == '?')
            {
                regex += ".";
            }
            else
            {
                regex += c;
            }
        }

        // 5) Anchor it
        return "^" + regex + "$";
    }
}
