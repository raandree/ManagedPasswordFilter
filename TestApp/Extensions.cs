using System.Runtime.InteropServices;

namespace TestApp
{
    public static class Extensions
    {
        public static UNICODE_STRING ToUnicodeString(this string s)
        {
            ushort length = (ushort)(s.Length * 2);
            ushort maximumlength = (ushort)(length + 2);
            UNICODE_STRING unicode_string = new UNICODE_STRING();
            unicode_string.Buffer = Marshal.StringToHGlobalUni(s);
            unicode_string.Length = length;
            unicode_string.MaximumLength = maximumlength;
            return unicode_string;
        }
    }
}
