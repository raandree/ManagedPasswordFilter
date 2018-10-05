using CommandLine;
using System;
using System.Runtime.InteropServices;

namespace TestApp
{

    public struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    public class CmdArguments
    {
        public enum TargetPlatform
        {
            ManagedWorker = 0,
            ManagedProxy = 1,
            UnmanagedProxy = 2
        }

        [CommandLineArgument(HelpTextLong = "Password", HelpTextShort = "Password to test", Type = ArgumentType.Required)]
        public string Password;

        [CommandLineArgument(HelpTextLong = "AccountName", HelpTextShort = "...", Type = ArgumentType.AtMostOnce)]
        public string AccountName = "TestAccountName";

        [CommandLineArgument(HelpTextLong = "FullName", HelpTextShort = "...", Type = ArgumentType.AtMostOnce)]
        public string FullName = "TestFullName";

        [CommandLineArgument(HelpTextLong = "", HelpTextShort = "Target Platform", Type = ArgumentType.AtMostOnce)]
        public TargetPlatform? Platform = TargetPlatform.ManagedWorker;

        [CommandLineArgument(HelpTextLong = "Writes a new MfpConfig file in C:\\Windows\\System32", HelpTextShort = "Init new MpfConfig", Type = ArgumentType.AtMostOnce)]
        public bool? InitConfig;

        [Application(Title = "password filter testapp", Version = "0.10", Description1 = "...")]
        private string App;
    }

    class Program
    {
        [DllImport("PwdFlt.dll")]
        public static extern bool PasswordFilter(
        ref UNICODE_STRING UserName,
        ref UNICODE_STRING FullName,
        ref UNICODE_STRING Password,
        bool SetOperation
        );

        static CommandLineReturnValues cmdResult;
        static GetCommandLineParameters cmd;
        static CmdArguments Parameters;

        static int Main(string[] args)
        {
            Parameters = new CmdArguments();
            cmd = new GetCommandLineParameters(args, Parameters);
            cmd.PrintApplicationDescription();
            cmdResult = cmd.ReadCommandLineParameters();
            if (cmdResult == CommandLineReturnValues.ShowHelp || cmdResult == CommandLineReturnValues.NoParametersGiven)
            {
                cmd.PrintHelp();
                return (int)cmdResult;
            }
            else if (cmdResult == CommandLineReturnValues.ShowExtendedHelp)
            {
                cmd.PrintExtendedHelp();
                return (int)cmdResult;
            }
            else if (cmdResult != CommandLineReturnValues.OK)
            {
                //cmd.PrintHelp();
                Console.WriteLine("Error parsing the input, exiting...");
                return (int)cmdResult;
            }
            else if (cmdResult == CommandLineReturnValues.OK)
            {
                cmd.PrintParameters();
            }

            //-----------------------------------------------------------------------------------------------------------------------------

            if (Parameters.InitConfig.HasValue)
            {
                Mpf.MpfConfig c = new Mpf.MpfConfig
                {
                    BlackListPath = "C:\\Windows\\System32\\BlackList.txt",
                    IsEnabled = true,
                    ResultIfFailure = true
                };
                c.PasswordPolicy.Denysettings = Mpf.PasswordSettings.DenyName | Mpf.PasswordSettings.DenyYear;
                c.PasswordPolicy.MinLength = 12;
                c.PasswordPolicy.MaxLength = 250;
                c.PasswordPolicy.MinScore = 3;
                c.PasswordPolicy.MaxConsecutiveRepeatingCharacters = 5;
                c.Export("C:\\Windows\\System32\\MpfConfig.xml");
            }

            bool result = false;
            if (Parameters.Platform == CmdArguments.TargetPlatform.ManagedWorker)
            {
                result = Mpf.Worker.TestPassword(Parameters.AccountName, Parameters.FullName, Parameters.Password);
            }
            else if (Parameters.Platform == CmdArguments.TargetPlatform.ManagedProxy)
            {
                Mpf.IProxy p = new Mpf.Proxy();
                result = p.TestPassword(Parameters.AccountName, Parameters.FullName, Parameters.Password);
            }
            else
            {
                var unicodeAccountName = Parameters.AccountName.ToUnicodeString();
                var unicodeFullName = Parameters.FullName.ToUnicodeString();
                var unicodePassword = Parameters.Password.ToUnicodeString();
                result = PasswordFilter(ref unicodeAccountName, ref unicodeFullName, ref unicodePassword, true);
            }

            Console.WriteLine(result);
            return Convert.ToInt32(result);
        }
    }
}