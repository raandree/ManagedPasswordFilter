using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CommandLine
{
    public enum CommandLineReturnValues : int
    {
        OK = 0,
        UndefinedError = -1,
        UnknownParameters = 5,
        SynatxError = 6,
        ConversionError = 7,
        ShowHelp = 10,
        ShowExtendedHelp = 11,
        NoParametersGiven = 12,
        MandatoryParameterMissing = 13,
        MandatorySlaveParameterMissing = 14
    }

    #region Classes
    public class CommandLineAttribute : Attribute
    {
        public ArgumentType Type;
        public string HelpTextShort;
        public string HelpTextLong;
        public string MasterArgument;
        public string MasterArgumentValue;
    }

    public class ApplicationAttribute : Attribute
    {
        public string Title;
        public string Version;
        public string Description1;
        public string Description2;
        public string Description3;
    }

    public class CommandLineArgument : CommandLineAttribute
    {
        public string Name;
        public string Value;
    }

    public class CommandLineArgumentCollection : CollectionBase
    {
        public virtual void Add(CommandLineArgument arg)
        {
            this.List.Add(arg);
        }

        public virtual void Remove(CommandLineArgument arg)
        {
            this.List.Remove(arg);
        }

        public virtual CommandLineArgument this[int Index]
        {
            get
            {
                return (CommandLineArgument)this.List[Index];
            }
        }

        public bool Contains(CommandLineArgument arg)
        {
            for (int i = 0; i < this.List.Count; i++)
            {
                CommandLineArgument a = (CommandLineArgument)List[i];
                if (a.Name == arg.Name &
                    a.Value == arg.Value)
                    return true;
            }
            return false;
        }

        public int Index(CommandLineArgument arg)
        {
            for (int i = 0; i < this.List.Count; i++)
            {
                CommandLineArgument a = (CommandLineArgument)List[i];
                if (a.Name == arg.Name &
                    a.Value == arg.Value)
                    return i;
            }
            return 0;
        }
    }
    #endregion

    #region ArgumentType
    /// <summary>
    /// Used to control parsing of command line arguments.
    /// </summary>
    [Flags]
    public enum ArgumentType
    {
        /// <summary>
        /// Indicates that this field is required. An error will be displayed
        /// if it is not present when parsing arguments.
        /// </summary>
        Required = 0x01,
        /// <summary>
        /// Only valid in conjunction with Multiple.
        /// Duplicate values will result in an error.
        /// </summary>
        Unique = 0x02,
        /// <summary>
        /// Inidicates that the argument may be specified more than once.
        /// Only valid if the argument is a collection
        /// </summary>
        Multiple = 0x04,

        /// <summary>
        /// The default type for non-collection arguments.
        /// The argument is not required, but an error will be reported if it is specified more than once.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// For non-collection arguments, when the argument is specified more than
        /// once no error is reported and the value of the argument is the last
        /// value which occurs in the argument list.
        /// </summary>
        LastOccurenceWins = Multiple,

        /// <summary>
        /// The default type for collection arguments.
        /// The argument is permitted to occur multiple times, but duplicate 
        /// values will cause an error to be reported.
        /// </summary>
        MultipleUnique = Multiple | Unique,

        /// <summary>
        /// Makes an argument required if the master attribute is present.
        /// Using this option a master - 'mandatory slave' relationship can be created
        /// </summary>
        RequiredSlave = 0x08,
    }
    #endregion

    #region ArgsDictionary
    public class ArgsDictionary : DictionaryBase
    {
        public CommandLineArgument this[string key]
        {
            get { return (CommandLineArgument)this.Dictionary[key]; }

            set { this.Dictionary[key] = value; }
        }

        public void Add(string key, CommandLineArgument arg)
        {
            this.Dictionary.Add(key, arg);
        }

        public bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        public ICollection Keys
        {
            get { return this.Dictionary.Keys; }
        }
    }
    #endregion

    #region GetCommandLineParameters
    public class GetCommandLineParameters
    {
        private string[] args;
        private int argsCount;
        private CommandLineArgumentCollection argCollection = new CommandLineArgumentCollection();
        private object parameterDefinitions; //that is the user defined object containing all the parameter / argument definitions
        private string unknownParameters = string.Empty;
        private string missingParameters = string.Empty;
        private int maxParameterNameLength = 0;
        private int WindowWidth = 0;

        public GetCommandLineParameters(string[] args, object ParameterDefinition)
        {
            this.args = args;
            this.argsCount = args.Length;
            this.parameterDefinitions = ParameterDefinition;

            try
            {
                WindowWidth = Console.WindowWidth;
            }
            catch
            {
                WindowWidth = 80; //Windows default
            }

            //determine the maximum length of the parameter names. This is for printing the help...
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields();
            foreach (FieldInfo fi in fic)
            {
                if (fi.Name.Length > maxParameterNameLength)
                    maxParameterNameLength = fi.Name.Length;
            }
            maxParameterNameLength = maxParameterNameLength + 4;
        }

        #region ReadCommandLineParameters
        public CommandLineReturnValues ReadCommandLineParameters()
        {
            CommandLineReturnValues result;

            //check if there are arguments to handle at all
            if (args.Length == 0)
            {
                return CommandLineReturnValues.NoParametersGiven;
            }

            //determine wether to show the help
            foreach (string s in args)
            {
                if (s.ToLower() == "/?" || s.ToLower() == "/help")
                    return CommandLineReturnValues.ShowHelp;
                if (s.ToLower() == "/??" || s.ToLower() == "extendedhelp")
                    return CommandLineReturnValues.ShowExtendedHelp;
            }

            result = SplitParameters(args, out argCollection);
            if (result != CommandLineReturnValues.OK) return result;

            //check for unknown paramters
            ContainsUnknownParameter(argCollection, parameterDefinitions, out unknownParameters);
            if (unknownParameters != "")
            {
                Console.WriteLine("The parameter(s) [{0}] are unknown", unknownParameters);
                Console.WriteLine("");
                return CommandLineReturnValues.UnknownParameters;
            }

            //transfer the values into the object that holds the parameter definition
            result = TransferParameterValues(this.argCollection, this.parameterDefinitions);
            if (result != CommandLineReturnValues.OK)
            {
                Console.WriteLine("Error converting one parameter.");
                return result;
            }

            ContainsAllMandatoryParameter(argCollection, parameterDefinitions, out missingParameters);

            if (missingParameters != "")
            {
                Console.WriteLine("The mandatory parameter(s) [{0}] are missing", missingParameters);
                Console.WriteLine("");
                return CommandLineReturnValues.MandatoryParameterMissing;
            }

            Dictionary<string, string> missingMandatorySlaveParameters;
            ContainsAllMandatorySlaveParameter(argCollection, parameterDefinitions, out missingMandatorySlaveParameters);
            if (missingMandatorySlaveParameters.Count != 0)
            {
                Console.WriteLine("There are {0} parameters missing", missingMandatorySlaveParameters.Count);
                foreach (KeyValuePair<string, string> kvp in missingMandatorySlaveParameters)
                {
                    Console.WriteLine("The parameter '{0}' is required if '{1}'", kvp.Value, kvp.Key);
                }
                Console.WriteLine("");
                return CommandLineReturnValues.MandatorySlaveParameterMissing;
            }
            Console.WriteLine("");
            return 0;
        }
        #endregion

        #region Print Info
        #region PrintUsage
        public void PrintUsage()
        {
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields();

            string appName = string.Empty;

            foreach (FieldInfo fi in fic)
            {
                object[] customAttributes = fi.GetCustomAttributes(false);
                CommandLineArgument ca = (CommandLineArgument)customAttributes[0];
                if (ca.Type == ArgumentType.Required)
                    Console.Write("/{0}{1}", fi.Name, fi.FieldType == typeof(Nullable<bool>) ? "" : ":");
                else if (ca.Type == ArgumentType.RequiredSlave)
                {
                    Console.Write("/{0}{1}", fi.Name, fi.FieldType == typeof(Nullable<bool>) ? "" : ":");
                }
                else
                    Console.Write("[/{0}{1}]", fi.Name, fi.FieldType == typeof(Nullable<bool>) ? "" : ":");

                Console.Write(" ");
            }
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }
        #endregion

        #region PrintParameters
        public void PrintParameters()
        {
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields();

            foreach (FieldInfo fi in fic)
            {
                Console.Write(fi.Name +
                            new string(' ', maxParameterNameLength - fi.Name.Length) +
                            (fi.GetValue(parameterDefinitions) == null ? "<not set>" : fi.GetValue(parameterDefinitions).ToString()) +
                            Environment.NewLine);
            }
            Console.WriteLine(new string('-', WindowWidth));

        }
        #endregion

        #region PrintHelp
        public void PrintHelp()
        {
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields();
            object[] customAttributes;

            foreach (FieldInfo fi in fic)
            {
                customAttributes = fi.GetCustomAttributes(false);

                foreach (CommandLineArgument customAttribute in customAttributes)
                {
                    if ((customAttribute.HelpTextShort != ""))
                    {
                        Console.Write("{0}{1}",
                            fi.Name,
                            new string(' ', maxParameterNameLength - fi.Name.Length));

                        string[] words = customAttribute.HelpTextShort.Split(' ');
                        int availableCharacters = WindowWidth - maxParameterNameLength;
                        int remainingCharacters = availableCharacters;

                        foreach (string w in words)
                        {

                            if (w.Length < remainingCharacters)
                            {
                                Console.Write(w + " ");
                                remainingCharacters = remainingCharacters - (w.Length + 1);
                            }
                            else
                            {
                                //Console.Write(Environment.NewLine);
                                Console.Write(new string(' ', maxParameterNameLength));
                                Console.Write(w + " ");
                                remainingCharacters = availableCharacters - (w.Length + 1);
                            }
                        }
                        if (fi.FieldType.IsGenericType)
                        {
                            string[] names = null;
                            if (fi.FieldType.GetGenericArguments()[0].BaseType == typeof(Enum))
                            {
                                names = Enum.GetNames(fi.FieldType.GetGenericArguments()[0]);
                                remainingCharacters = availableCharacters - (" - Valed values are: ".Length + 1);

                                Console.Write(Environment.NewLine + new string(' ', maxParameterNameLength) + " - Valed values are: ");
                                foreach (string s in names)
                                {

                                    if (s.Length + 1 < remainingCharacters)
                                    {
                                        Console.Write(s + " ");
                                        remainingCharacters = remainingCharacters - (s.Length + 1);
                                    }
                                    else
                                    {
                                        Console.Write(Environment.NewLine);
                                        Console.Write(new string(' ', maxParameterNameLength));
                                        Console.Write(s + " ");
                                        remainingCharacters = availableCharacters - (s.Length + 1);
                                    }
                                }
                                Console.WriteLine("");
                            }
                        }
                        Console.WriteLine("");
                    }
                }
            }
            Console.WriteLine(new string('-', WindowWidth));
        }
        #endregion

        #region PrintExtendedHelp
        public void PrintExtendedHelp()
        {
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields();
            object[] customAttributes;

            foreach (FieldInfo fi in fic)
            {
                customAttributes = fi.GetCustomAttributes(false);

                foreach (CommandLineArgument customAttribute in customAttributes)
                {
                    if ((customAttribute.HelpTextShort != ""))
                    {
                        Console.Write("{0}{1}",
                            fi.Name,
                            new string(' ', maxParameterNameLength - fi.Name.Length));

                        string[] words = customAttribute.HelpTextLong.Split(' ');
                        int availableCharacters = WindowWidth - maxParameterNameLength;
                        int remainingCharacters = availableCharacters;

                        foreach (string w in words)
                        {

                            if (w.Length < remainingCharacters)
                            {
                                Console.Write(w + " ");
                                remainingCharacters = remainingCharacters - (w.Length + 1);
                            }
                            else
                            {
                                //Console.Write(Environment.NewLine);
                                Console.Write(new string(' ', maxParameterNameLength));
                                Console.Write(w + " ");
                                remainingCharacters = availableCharacters - (w.Length + 1);
                            }
                        }
                        if (fi.FieldType.IsGenericType)
                        {
                            string[] names = null;
                            if (fi.FieldType.GetGenericArguments()[0].BaseType == typeof(Enum))
                            {
                                names = Enum.GetNames(fi.FieldType.GetGenericArguments()[0]);
                                remainingCharacters = availableCharacters - (" - Valed values are: ".Length + 1);

                                Console.Write(Environment.NewLine + new string(' ', maxParameterNameLength) + " - Valed values are: ");
                                foreach (string s in names)
                                {

                                    if (s.Length + 1 < remainingCharacters)
                                    {
                                        Console.Write(s + " ");
                                        remainingCharacters = remainingCharacters - (s.Length + 1);
                                    }
                                    else
                                    {
                                        Console.Write(Environment.NewLine);
                                        Console.Write(new string(' ', maxParameterNameLength));
                                        Console.Write(s + " ");
                                        remainingCharacters = availableCharacters - (s.Length + 1);
                                    }
                                }
                                Console.WriteLine("");
                            }
                        }
                        Console.WriteLine("");
                    }
                }
            }
            Console.WriteLine(new string('-', WindowWidth));
        }
        #endregion

        #region PrintApplicationDescription
        public void PrintApplicationDescription()
        {
            FieldInfo[] fic = parameterDefinitions.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            object[] customAttributes;

            foreach (FieldInfo fi in fic)
            {
                if (fi.Name == "App")
                {
                    customAttributes = fi.GetCustomAttributes(false);

                    //print title
                    Console.WriteLine("");
                    Console.WriteLine(new string('-', WindowWidth));
                    ApplicationAttribute aa = (ApplicationAttribute)customAttributes[0];
                    Console.WriteLine("{0} {1}", aa.Title, aa.Version);

                    if (aa.Description1 != "" || aa.Description1 != null)
                    {
                        Console.WriteLine("");
                        Console.WriteLine(aa.Description1);
                    }
                    if (aa.Description2 != "" && aa.Description2 != null)
                    {
                        Console.WriteLine("");
                        Console.WriteLine(aa.Description2);
                    }
                    if (aa.Description3 != "" && aa.Description3 != null)
                    {
                        Console.WriteLine("");
                        Console.WriteLine(aa.Description3);
                    }
                    Console.WriteLine(new string('-', WindowWidth));
                }
            }
        }
        #endregion
        #endregion

        #region ContainsUnknownParameter
        private int ContainsUnknownParameter(CommandLineArgumentCollection args, object argsdef, out List<string> UnknownParameters)
        {
            UnknownParameters = new List<string>();

            FieldInfo[] fic = argsdef.GetType().GetFields();

            foreach (CommandLineArgument arg in args)
            {
                bool isUnknown = true;

                foreach (FieldInfo fi in fic)
                {
                    if (fi.Name.ToLower() == arg.Name.ToLower())
                        isUnknown = false;
                }

                if (isUnknown)
                {
                    UnknownParameters.Add(arg.Name);
                }
            }
            return (int)CommandLineReturnValues.OK;
        }

        private int ContainsUnknownParameter(CommandLineArgumentCollection args, object argsdef, out string UnknownParameters)
        {
            int result = 0;
            UnknownParameters = string.Empty;

            List<string> unknownParametersList;

            result = ContainsUnknownParameter(args, argsdef, out unknownParametersList);

            if (result != 0) return result;

            foreach (string s in unknownParametersList)
            {
                UnknownParameters += s + ", ";
            }
            if (UnknownParameters.EndsWith(", ")) UnknownParameters = UnknownParameters.Substring(0, UnknownParameters.Length - 2);

            return (int)CommandLineReturnValues.OK;
        }
        #endregion

        #region TransferParameterValues
        private CommandLineReturnValues TransferParameterValues(CommandLineArgumentCollection args, object TargetObject)
        {
            foreach (CommandLineArgument arg in args)
            {
                FieldInfo[] fic = TargetObject.GetType().GetFields();

                foreach (FieldInfo fi in fic)
                {
                    if (fi.Name.ToLower() == arg.Name.ToLower())
                    {
                        try
                        {
                            if (fi.FieldType.IsGenericType)
                            {
                                if (fi.FieldType.GetGenericArguments()[0].BaseType == typeof(Enum))
                                    fi.SetValue(parameterDefinitions, Enum.Parse(fi.FieldType.GetGenericArguments()[0], arg.Value, true));
                                if (fi.FieldType == typeof(Nullable<int>))
                                    fi.SetValue(parameterDefinitions, Convert.ToInt32(arg.Value));
                                if (fi.FieldType == typeof(Nullable<bool>))
                                    fi.SetValue(parameterDefinitions, Convert.ToBoolean(arg.Value));
                            }
                            else if (fi.FieldType == typeof(string))
                                fi.SetValue(parameterDefinitions, arg.Value);
                            else if (fi.FieldType.BaseType == typeof(Array))
                            {
                                if (fi.FieldType.GetElementType() == typeof(int))
                                {
                                    string[] a = arg.Value.Split(',');

                                    int[] values =
                                        Array.ConvertAll<string, int>(a, new Converter<string, int>(Convert.ToInt32));

                                    fi.SetValue(TargetObject, values);
                                }
                                else if (fi.FieldType.GetElementType() == typeof(bool))
                                {
                                    throw new NotImplementedException();
                                    //string[] a = arg.Value.Split(',');

                                    //int[] i =
                                    //    Array.ConvertAll<string, int>(a, new Converter<string, int>(Convert.ToInt32));

                                    //int[] values =
                                    //    Array.ConvertAll<int, bool>(i, new Converter<int, bool>(Convert.ToBoolean));

                                    //fi.SetValue(TargetObject, values);

                                    //string y = "1";
                                    //bool x = Convert.ToBoolean(y);
                                }
                                else if (fi.FieldType.GetElementType() == typeof(string))
                                {
                                    string[] a = arg.Value.Split(',');

                                    fi.SetValue(TargetObject, a);
                                }
                            }

                            else
                            {
                                fi.SetValue(TargetObject, arg.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Could not convert the value for parameter '{0}' to '{1}'. The error was '{2}'", arg.Name,
                                fi.FieldType.IsGenericType ? fi.FieldType.GetGenericArguments()[0].FullName : fi.FieldType.Name, ex.Message);

                            return CommandLineReturnValues.ConversionError;
                        }
                    }
                }
            }

            return CommandLineReturnValues.OK;
        }
        #endregion

        #region ContainsAllMandatoryParameter
        private CommandLineReturnValues ContainsAllMandatoryParameter(CommandLineArgumentCollection args, object argsdef, out List<string> MissingManadatoryParameters)
        {
            MissingManadatoryParameters = new List<string>();

            FieldInfo[] fic = argsdef.GetType().GetFields();

            foreach (FieldInfo fi in fic)
            {
                object[] customAttributes = fi.GetCustomAttributes(false);
                foreach (CommandLineArgument customAttribute in customAttributes)
                {
                    if ((customAttribute.Type & ArgumentType.Required) == ArgumentType.Required)
                    {
                        if (fi.GetValue(argsdef) == null)
                            MissingManadatoryParameters.Add(fi.Name);
                    }
                }
            }

            return 0;
        }

        private CommandLineReturnValues ContainsAllMandatoryParameter(CommandLineArgumentCollection args, object argsdef, out string MissingManadatoryParameters)
        {
            CommandLineReturnValues result;
            MissingManadatoryParameters = string.Empty;

            List<string> MissingManadatoryParameterList;

            result = ContainsAllMandatoryParameter(args, argsdef, out MissingManadatoryParameterList);

            if (result != 0) return result;

            foreach (string s in MissingManadatoryParameterList)
            {
                MissingManadatoryParameters += s + ", ";
            }
            if (MissingManadatoryParameters.EndsWith(", ")) MissingManadatoryParameters = MissingManadatoryParameters.Substring(0, MissingManadatoryParameters.Length - 2);

            return CommandLineReturnValues.OK;
        }
        #endregion

        #region ContainsAllMandatorySlaveParameter
        private int ContainsAllMandatorySlaveParameter(CommandLineArgumentCollection args, object argsdef, out Dictionary<string, string> MissingManadatorySlaveParameters)
        {
            MissingManadatorySlaveParameters = new Dictionary<string, string>();
            string[] masterArgumentValueArray;

            FieldInfo[] fic = argsdef.GetType().GetFields();

            //for all the defined attributes in the user defined class
            foreach (FieldInfo fi in fic)
            {
                //get the custom attributes
                object[] customAttributes = fi.GetCustomAttributes(false);
                foreach (CommandLineArgument customAttribute in customAttributes)
                {
                    //if the attribut is RequiredSlave and check if the MasterArgumentValue is present for the RequiredSlave argument
                    if (
                        ((customAttribute.Type & ArgumentType.RequiredSlave) == ArgumentType.RequiredSlave) &&
                        (customAttribute.MasterArgument != null && customAttribute.MasterArgument != ""))
                    {
                        //go through the parameters and
                        foreach (CommandLineArgument ca in args)
                        {
                            //get the list of master parameters
                            string[] MasterParameterNames = customAttribute.MasterArgument.ToLower().Split(',');

                            //for each master parameter defined for the parameter 'fi'
                            for (int iMasterParameters = 0; iMasterParameters < MasterParameterNames.Length; iMasterParameters++)

                                //check if the master parameters is present
                                if (ca.Name.ToLower() == MasterParameterNames[iMasterParameters])
                                {
                                    //verify if MasterArgumentValue is defined
                                    if (customAttribute.MasterArgumentValue == null)
                                    {
                                        //if not
                                        //check if there is some value at all given
                                        if (fi.GetValue(argsdef) == null)
                                        {
                                            //if not, try to add the parameter to the MissingManadatorySlaveParameters list
                                            try
                                            {
                                                MissingManadatorySlaveParameters.Add(
                                                    MasterParameterNames[iMasterParameters] + "=*" + customAttribute.MasterArgumentValue,
                                                    fi.Name);
                                            }
                                            catch { }
                                        }
                                        //if we have some value we can go to the next parameter
                                        break;
                                    }
                                    //verify if there are many values possible for MasterArgumentValue
                                    if (customAttribute.MasterArgumentValue.Contains(","))
                                    {
                                        masterArgumentValueArray = customAttribute.MasterArgumentValue.Split(',');
                                        foreach (string s in masterArgumentValueArray)
                                        {
                                            if (argsdef.GetType().GetField(customAttribute.MasterArgument).GetValue(argsdef).ToString().ToLower() ==
                                                s.ToLower())
                                                if (fi.GetValue(argsdef) == null)
                                                {
                                                    //check if the paramater is already in the dictionary MissingManadatorySlaveParameters
                                                    string temp;
                                                    if (!MissingManadatorySlaveParameters.TryGetValue(
                                                        customAttribute.MasterArgument + "=" + customAttribute.MasterArgumentValue, out temp))
                                                        MissingManadatorySlaveParameters.Add(
                                                            customAttribute.MasterArgument + "=" + customAttribute.MasterArgumentValue,
                                                            fi.Name);
                                                }
                                        }
                                    }
                                    // if MasterArgumentValue is not defined
                                    else
                                    {
                                        if (argsdef.GetType().GetField(customAttribute.MasterArgument).GetValue(argsdef).ToString().ToLower() ==
                                                customAttribute.MasterArgumentValue.ToLower())
                                            if (fi.GetValue(argsdef) == null)
                                                MissingManadatorySlaveParameters.Add(
                                                    customAttribute.MasterArgument + "=" + customAttribute.MasterArgumentValue,
                                                    fi.Name);
                                    }
                                }
                        }
                    }
                }
            }

            return 0;
        }
        #endregion

        #region SplitParameters
        private CommandLineReturnValues SplitParameters(string[] args, out CommandLineArgumentCollection ArgumentCollection)
        {
            ArgumentCollection = new CommandLineArgumentCollection();

            for (int i = 0; i <= (int)args.LongLength - 1; i++)
            {
                CommandLineArgument arg = new CommandLineArgument();

                //check if the element starts with a '/'
                if (!args[i].StartsWith("/"))
                    return CommandLineReturnValues.SynatxError;

                //check for args like '/Mailbox:' that are normally strings or integers
                if (args[i].LastIndexOf(":") > 0)
                {
                    arg.Name = args[i].Substring(1, args[i].IndexOf(":") - 1).ToLower();
                    if (arg.Name.Contains("password"))
                    {
                        arg.Value = args[i].Substring(args[i].IndexOf(":") + 1);
                    }
                    else
                    {
                        arg.Value = args[i].Substring(args[i].IndexOf(":") + 1).ToLower();
                    }
                }
                else //check for args like switches /doThis
                {
                    arg.Name = args[i].Substring(1).ToLower();
                    arg.Value = "true";
                }

                //add the argument to the collection
                try
                {
                    ArgumentCollection.Add(arg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not add argument to collection. Error: {0}", ex.Message);
                    return CommandLineReturnValues.UndefinedError;
                }
                arg = null;
            }

            return CommandLineReturnValues.OK;
        }
        #endregion
    }
    #endregion
}