using Mpf.Stores;
using System;

namespace Mpf
{
    [Serializable]
    public class Config : XmlStore<Config>
    {
        public Config()
        {
            PasswordPolicy = new PasswordPoliciy();
        }

        public bool IsEnabled { get; set; }
        public bool ResultIfFailure { get; set; }
        public string BlackListPath { get; set; }
        public PasswordPoliciy PasswordPolicy { get; set; }
    }

    [Serializable]
    public class PasswordPoliciy
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public int MinScore { get; set; }
        public int MaxConsecutiveRepeatingCharacters { get; set; }
        public PasswordSettings DenySettings { get; set; }
        public double? AllowedBlackListQuotaPercent { get; set; }
    }

    [Flags]
    public enum PasswordSettings
    {
        DenyGivenName = 1,
        DenySurname = 2,
        DenyName = DenyGivenName | DenySurname
    }
}