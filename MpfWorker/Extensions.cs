using System.Text.RegularExpressions;

namespace Mpf
{
    public static class Extensions
    {
        public static string WildCardToRegular(this string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public static int WildCardMatch(this string value, string pattern, bool ignoreCase = true)
        {
            if (ignoreCase)
                return Regex.Matches(value, Regex.Replace(pattern, @"\*|\?", ""), RegexOptions.IgnoreCase).Count;

            return Regex.Matches(value, WildCardToRegular(pattern)).Count;
        }

        public static bool HasConsecutiveChars(this string value, int sequenceLength)
        {
            return !Regex.IsMatch(value, "(.)\\1{" + (sequenceLength - 1) + "}");
        }
    }
}
