using Mpf.Stores;
using System;
using System.DirectoryServices;
using System.IO;

namespace Mpf
{
    public static class Worker
    {
        static MpfConfig config = null;
        static Zxcvbn.Zxcvbn Zxcvbn = new Zxcvbn.Zxcvbn();
        static DirectoryEntry rootDse = null;

        static Worker()
        {
            try
            {
                config = XmlStore<MpfConfig>.Import("C:\\Windows\\System32\\MpfConfig.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not open 'C:\\Windows\\System32\\MpfConfig.xml', the error was: {0}", ex.Message));
                config = new MpfConfig
                {
                    IsEnabled = true,
                    ResultIfFailure = true
                };
                config.PasswordPolicy.MinLength = 12;
                config.PasswordPolicy.MinScore = 4;
                config.PasswordPolicy.MaxConsecutiveRepeatingCharacters = 5;
            }

            try
            {
                rootDse = new DirectoryEntry("LDAP://RootDSE");
            }
            catch
            {
                Console.WriteLine("Could not read RootDse object, AD Tests will be skipped.");
            }
        }

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

        static bool TestBlackList(string password)
        {
            if (!File.Exists(config.BlackListPath))
                return config.ResultIfFailure;

            var result = true;

            using (var sr = new StreamReader(config.BlackListPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().ToLower();
                    if (line.Contains("*"))
                    {
                        var xx = password.WildCardMatch(line);
                        if (password.ToLower().Contains(line.Replace("*", "")))
                            return false;
                    }
                    else
                    {
                        if (password.ToLower() == line)
                            return false;
                    }
                }
            }
            return result;
        }

        static bool TestAdAttributeValueName(string accountName, string propertyName, string password)
        {
            try
            {
                var ds = new DirectorySearcher(rootDse.Properties["defaultNamingContext"].Value.ToString())
                {
                    Filter = string.Format("(&(objectCategory=person)(samAccountName={0}))", accountName)
                };
                var sr = ds.FindOne();
                var value = sr.GetDirectoryEntry().Properties[propertyName][0].ToString();
                Console.WriteLine("AD Value is: " + value);
                if (password.ToLower().Contains(value.ToLower()))
                    return false;
                else
                    return true;
            }
            catch
            {
                return config.ResultIfFailure; ;
            }
        }
    }
}