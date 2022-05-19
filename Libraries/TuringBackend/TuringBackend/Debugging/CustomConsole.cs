using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace TuringBackend.Debugging
{
    public static class CustomConsole
    {
        public delegate void LogMethod(string Message);
        public static LogMethod LogPointer = delegate (string Message) { Debug.WriteLine(Message); };//  new LogMethod(Debug.WriteLine);
        public static LogMethod WritePointer = delegate (string Message) { Debug.Write(Message); };//  new LogMethod(Debug.WriteLine);

        static string LogFilePath;
        static FileStream LogStream;

        static CustomConsole()
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Turing Machine - Desktop");
            LogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Turing Machine - Desktop\\Log.txt";
            LogStream = File.OpenWrite(LogFilePath);
        }

        public static void Log(string Message)
        {           
            LogPointer(Message);
            LogStream.Write(Encoding.ASCII.GetBytes(Message+"\n"));
            LogStream.Flush();
        }
        public static void Write(string Message)
        {
            WritePointer(Message);
            LogStream.Write(Encoding.ASCII.GetBytes(Message));
            LogStream.Flush();
        }

    }
}
