using System;
using Mpf;

[PasswordRuleContainer(Order = 99)]
public class QuotaPasswordRules : PasswordRulesBase
{
    [PasswordRule]
    public static bool TestTotalProhibitedCharacters(string password, string accountName)
    {
        if (Chars > 0)
        {
            var characterQuotaPercentage = Convert.ToDouble(Chars) / password.Length * 100;
            Console.WriteLine("Total quota is {0:N2}%, policy is {1}%", characterQuotaPercentage, config.PasswordPolicy.AllowedBlackListQuotaPercent);
            return characterQuotaPercentage < config.PasswordPolicy.AllowedBlackListQuotaPercent;
        }
        else
        {
            return true;
        }
    }
}