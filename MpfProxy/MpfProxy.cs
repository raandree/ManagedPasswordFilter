using System;
using System.Runtime.InteropServices;

namespace Mpf
{
    [Guid("03AD5D2D-2AFD-439f-8713-A4EC0705B4D9")]
    public interface IProxy
    {
        bool TestPassword(string accountName, string fullName, string password);
    }

    [Guid("0490E147-F2D2-4909-A4B8-3533D2F264D0")]
    public class Proxy : IProxy, IDisposable
    {
        public void Dispose()
        { }

        bool IProxy.TestPassword(string accountName, string fullName, string password)
        {
            bool result = Worker.TestPassword(accountName, fullName, password);

            return result;
        }
    }
}
