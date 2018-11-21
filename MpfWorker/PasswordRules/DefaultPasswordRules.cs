using Mpf;

[PasswordRuleContainer]
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
    public static bool TestMaxConsecutiveChars(string password, string accountName)
    {
        if (config.PasswordPolicy.MaxConsecutiveRepeatingCharacters > 0)
            return password.HasConsecutiveChars(config.PasswordPolicy.MaxConsecutiveRepeatingCharacters);
        else
            return true;
    }
}