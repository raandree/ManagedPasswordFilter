using Mpf.Stores;
using System;
using System.DirectoryServices;

namespace Mpf
{
    public class PasswordRulesBase
    {
        protected static Config config = null;
        protected static bool IsConnectedToAd = false;
        protected static DirectoryEntry rootDse = null;
        static string configFilePath = @"C:\Windows\System32\MpfConfig.xml";

        public bool IsEnabled
        {
            get { return config.IsEnabled; }
        }

        public PasswordRulesBase()
        {
            try
            {
                config = XmlStore<Config>.Import(configFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not open '{0}', the error was: {1}", configFilePath, ex.Message));
                config = new Config
                {
                    IsEnabled = true,
                    ResultIfFailure = true
                };
                config.PasswordPolicy.MinLength = 12;
                config.PasswordPolicy.MaxLength = 256;
                config.PasswordPolicy.MinScore = 4;
                config.PasswordPolicy.MaxConsecutiveRepeatingCharacters = 5;
            }

            try
            {
                rootDse = new DirectoryEntry("LDAP://RootDSE");
                var temp = rootDse.Name;
                IsConnectedToAd = true;
            }
            catch
            {
                Console.WriteLine("Could not read RootDse object, AD Tests will be skipped.");
            }
        }
    }
}