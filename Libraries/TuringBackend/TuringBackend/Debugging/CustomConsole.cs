using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TuringBackend.Debugging
{
    public static class CustomConsole
    {
        public delegate void LogMethod(string Message);
        public static LogMethod LogPointer = delegate (string Message) { Debug.WriteLine(Message); };//  new LogMethod(Debug.WriteLine);
        public static LogMethod AppendPointer = delegate (string Message) { Debug.Write(Message); };//  new LogMethod(Debug.WriteLine);

        public static void Log(string Message)
        {           
            LogPointer(Message);
            //add log to file later
        }

        public static void Write(string Message)
        {
            AppendPointer(Message);
        }
    }
}
