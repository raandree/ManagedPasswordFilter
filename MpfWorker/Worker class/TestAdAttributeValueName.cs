using System;
using System.DirectoryServices;

namespace Mpf
{
    public static partial class Worker
    {
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