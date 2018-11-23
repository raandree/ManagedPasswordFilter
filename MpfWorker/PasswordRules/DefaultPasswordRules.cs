using Mpf;

[PasswordRuleContainer(Order = 1)]
public class DefaultPasswordRules : PasswordRulesBase
{
    [PasswordRule]
    public static bool TestMinPasswordLength(string password, string accountName)
    {
        return password.Length >= config.PasswordPolicy.MinLength;
    }

    [PasswordRule]
    public static bool TestMaxPasswordLength(string password, string accountName)
    {
        return password.Length < config.PasswordPolicy.MaxLength;
    }

    [PasswordRule]
    public static bool TestAtLeastOneUpperCaseCharacter(string password, string accountName)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]+");
    }

    [PasswordRule]
    public static bool TestAtLeastOneLowerCaseCharacter(string password, string accountName)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]+");
    }

    [PasswordRule]
    public static bool TestMaxConsecutiveChars(string password, string accountName)
    {
        if (config.PasswordPolicy.MaxConsecutiveRepeatingCharacters > 0)
            return password.HasConsecutiveChars(config.PasswordPolicy.MaxConsecutiveRepeatingCharacters);
        else
            return true;
    }
}