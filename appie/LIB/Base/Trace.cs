using System.Diagnostics;

namespace System
{
    public class Tracer
    {
        const string _fix = "[TRACE] - ";
        
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            Debug.WriteLine(_fix + message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, object arg0)
        {
            Debug.WriteLine(string.Format(_fix + format, arg0));
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, object arg0, object arg1)
        {
            Debug.WriteLine(string.Format(_fix + format, arg0, arg1));
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Debug.WriteLine(string.Format(_fix + format, arg0, arg1, arg2));
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3)
        {
            Debug.WriteLine(string.Format(_fix + format, arg0, arg1, arg2, arg3)); 
        }
    }
}
