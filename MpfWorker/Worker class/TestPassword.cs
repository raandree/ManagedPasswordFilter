using System;

namespace Mpf
{
    public static partial class Worker
    {
        public static bool TestPassword(string accountName, string fullName, string password)
        {
            if (!config.IsEnabled)
                return true;

            var result = true;

            Console.WriteLine(string.Format("Password test for account '{0}'", accountName));
            if (config.BlackListPath != string.Empty)
            {
                Console.WriteLine("Blacklist check");
                result = result && TestBlackList(password);
            }

            if (result && config.PasswordPolicy.MinLength > 0)
            {
                Console.WriteLine("Min Length test");
                result &= password.Length >= config.PasswordPolicy.MinLength;
            }

            if (result && config.PasswordPolicy.MaxLength > 0)
            {
                Console.WriteLine("Max Length Test");
                result &= password.Length < config.PasswordPolicy.MaxLength;
            }

            if (result && config.PasswordPolicy.MaxConsecutiveRepeatingCharacters > 0)
            {
                Console.WriteLine("MaxConsecutiveRepeatingCharacters test");
                result &= password.HasConsecutiveChars(config.PasswordPolicy.MaxConsecutiveRepeatingCharacters);
            }

            if (result)
            {
                if (config.PasswordPolicy.MinScore > 0)
                {
                    Console.WriteLine("Password scrore test");
                    var r = Zxcvbn.EvaluatePassword(password);
                    Console.WriteLine(string.Format("\tPassword score is {0}", r.Score));
                    result &= r.Score >= config.PasswordPolicy.MinScore;
                }
            }

            if (result && Convert.ToBoolean(config.PasswordPolicy.Denysettings & PasswordSettings.DenyGivenName))
            {
                Console.WriteLine("DenyGivenName AD Test");
                result &= TestAdAttributeValueName(accountName, "givenName", password);
            }
            if (result && Convert.ToBoolean(config.PasswordPolicy.Denysettings & PasswordSettings.DenySurname))
            {
                Console.WriteLine("DenySurname AD Test");
                result &= TestAdAttributeValueName(accountName, "sn", password);
            }

            if (result && Convert.ToBoolean(config.PasswordPolicy.Denysettings & PasswordSettings.DenyYear))
            {
                result &= !password.Contains(DateTime.Now.Year.ToString());
            }

            return result;
        }
    }
}