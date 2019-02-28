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
        readonly string configFilePath = @"C:\Windows\MpfConfig.xml";

        public bool IsEnabled
        {
            get { return config.IsEnabled; }
        }

        protected static int Chars { get; set; }

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
                config.PasswordPolicy.AllowedBlackListQuotaPercent = 20;
                config.PasswordPolicy.DenySettings = PasswordSettings.DenyName;
            }

            try
            {
                rootDse = new DirectoryEntry("LDAP://RootDSE");
                var temp = rootDse.Name;
                IsConnectedToAd = true;
            }
            catch
            {
                //No connection to AD
            }
        }
    }
}