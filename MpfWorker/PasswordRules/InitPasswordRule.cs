using Mpf;

[PasswordRuleContainer(Order = 0)]
public class InitPasswordRule : PasswordRulesBase
{
    [PasswordRule]
    public static bool InitValues(string password, string accountName)
    {
        Chars = 0;
        return true;
    }
}