using System;
using Mpf;

[PasswordRuleContainer(Order = 10)]
public class ZxcvbnPasswordRules : PasswordRulesBase
{
    static Zxcvbn.Zxcvbn Zxcvbn = new Zxcvbn.Zxcvbn();

    [PasswordRule]
    public static bool TestZxcvbn(string password, string accountName)
    {
        if (config.PasswordPolicy.MinScore > 0)
        {
            var r = Zxcvbn.EvaluatePassword(password);
            Console.WriteLine(string.Format("\tPassword score is {0}", r.Score));
            return r.Score >= config.PasswordPolicy.MinScore;
        }
        else
            return true;
    }
}