using Mpf.Stores;
using System;

namespace Mpf
{
    [Serializable]
    public class MpfConfig : XmlStore<MpfConfig>
    {
        public MpfConfig()
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
        public PasswordSettings Denysettings { get; set; }
    }

    [Flags]
    public enum PasswordSettings
    {
        DenyGivenName = 1,
        DenySurname = 2,
        DenyName = DenyGivenName | DenySurname,
        DenyYear = 4
    }
}