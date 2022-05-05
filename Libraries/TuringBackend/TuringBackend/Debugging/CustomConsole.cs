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
        public static void Log(string Message)
        {
            Debug.WriteLine(Message);
        }

        public static void Write(string Message)
        {
            Debug.Write(Message);
        }
    }
}
