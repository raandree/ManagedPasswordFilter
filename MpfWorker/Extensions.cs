using System.Text.RegularExpressions;

namespace Mpf
{
    public static class Extensions
    {
        public static string WildCardToRegular(this string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public static bool WildCardMatch(this string value, string pattern, bool ignoreCase = true)
        {
            if (ignoreCase)
                return Regex.IsMatch(value, WildCardToRegular(pattern), RegexOptions.IgnoreCase);

            return Regex.IsMatch(value, WildCardToRegular(pattern));
        }

        public static bool HasConsecutiveChars(this string value, int sequenceLength)
        {
            return !Regex.IsMatch(value, "(.)\\1{" + (sequenceLength - 1) + "}");
        }
    }
}
