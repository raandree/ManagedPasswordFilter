using Mpf;
using System;
using System.DirectoryServices;

[PasswordRuleContainer]
public class AdPasswordRules : PasswordRulesBase
{
    [PasswordRule]
    public static bool TestDenyGivenName(string password, string accountName)
    {
        if (!IsConnectedToAd)
        {
            Console.WriteLine("Could not connect to AD, skipping test");
            return true;
        }

        if (Convert.ToBoolean(config.PasswordPolicy.Denysettings & PasswordSettings.DenyGivenName))
            return TestAdAttributeValueName(accountName, "givenName", password);
        else
            return true;
    }

    [PasswordRule]
    public static bool TestDenySurname(string password, string accountName)
    {
        if (!IsConnectedToAd)
        {
            Console.WriteLine("Could not connect to AD, skipping test");
            return true;
        }

        if (Convert.ToBoolean(config.PasswordPolicy.Denysettings & PasswordSettings.DenyGivenName))
            return TestAdAttributeValueName(accountName, "sn", password);
        else
            return true;
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