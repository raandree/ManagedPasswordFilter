using System;
using System.IO;
using Mpf;

[PasswordRuleContainer(Order = 3)]
public class BlackListPasswordRules : PasswordRulesBase
{
    [PasswordRule]
    public static bool TestBlackList(string password, string accountName)
    {
        if (!File.Exists(config.BlackListPath))
            return config.ResultIfFailure;

        using (var sr = new StreamReader(config.BlackListPath))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                var wcm = password.WildCardMatch(line, false);
                if (wcm > 0 && config.PasswordPolicy.AllowedBlackListQuotaPercent == null)
                {
                    //if AllowedBlackListQuotaPercent is null, not match from the blacklist is allowed in the password
                    Console.WriteLine("Password is in the blacklist");
                    return false;
                }
                else if (wcm > 0)
                {
                    //if AllowedBlackListQuotaPercent is set, the charaters matching the blacklist are accumulated.
                    //the AllowedBlackListQuotaPercent is checked at the end.
                    var characterCount = line.Replace("*", "").Length * wcm;
                    Console.WriteLine("Wildcard '{0}' matches the password, adding {1} characters to the quota", line, characterCount);
                    Chars += characterCount;
                }
            }
        }

        if (Chars > 0)
        {
            var blackListPercentage = Convert.ToDouble(Chars) / password.Length * 100;
            Console.WriteLine("Blacklist quota is {0:N2}%, policy is {1}%", blackListPercentage, config.PasswordPolicy.AllowedBlackListQuotaPercent);
            return blackListPercentage < config.PasswordPolicy.AllowedBlackListQuotaPercent;
        }
        else
        {
            return true;
        }
    }
}