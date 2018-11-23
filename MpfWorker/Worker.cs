using System;
using System.Linq;
using System.Reflection;

namespace Mpf
{
    public static class Worker
    {
        static int chars = 0;

        public static bool TestPassword(string accountName, string fullName, string password)
        {
            var result = true;
            var assembly = Assembly.GetAssembly(typeof(Worker));
            var classes = assembly.GetTypes()
                .Where(c => c.GetCustomAttributes(typeof(PasswordRuleContainerAttribute), false).Length > 0)
                .OrderBy(c => c.GetCustomAttributes<PasswordRuleContainerAttribute>().FirstOrDefault().Order)
                .ToArray();

            foreach (var c in classes)
            {
                var methods = c.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(PasswordRuleAttribute), false).Length > 0);

                Console.WriteLine("Going though {0} test rules in class '{1}'", methods.Count(), c.Name);

                var rules = (PasswordRulesBase)Activator.CreateInstance(c);
                if (rules.IsEnabled & methods.Count() > 0 & result)
                {
                    result = methods.ForEach(m =>
                    {                        
                        var r = Convert.ToBoolean(m.Invoke(c, new object[] { password, accountName }));
                        Console.WriteLine("Calling password filter rule '{0}': {1}", m.Name, r);
                        return r;
                    }).Min();
                }
                chars = (int)c.GetProperty("Chars", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(c);
            }

            return result;
        }
    }
}