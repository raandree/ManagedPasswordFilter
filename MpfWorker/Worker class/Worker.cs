using Mpf.Stores;
using System;
using System.DirectoryServices;

namespace Mpf
{
    public static partial class Worker
    {
        static Config config = null;
        static Zxcvbn.Zxcvbn Zxcvbn = new Zxcvbn.Zxcvbn();
        static DirectoryEntry rootDse = null;


        static Worker()
        {
            try
            {
                config = XmlStore<Config>.Import("C:\\Windows\\System32\\MpfConfig.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not open 'C:\\Windows\\System32\\MpfConfig.xml', the error was: {0}", ex.Message));
                config = new Config
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
    }
}