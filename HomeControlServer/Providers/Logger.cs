using System;
using System.IO;

namespace HomeControlServer.Providers
{
    public static class Logger
    {
        private const string LOGFILE_STEM = @"c:\HomeControl\Data\homecontrol_log";
        private const string LOGFILE_EXT = @".log";
        private const string LOGFILE = LOGFILE_STEM + LOGFILE_EXT;
        private const int MAXSIZE = 1000000;

        public const int LOGLEVEL_ERROR = 1;
        private const string LOGLEVEL_ERROR_STR = "ERROR";
        public const int LOGLEVEL_WARNING = 2;
        private const string LOGLEVEL_WARNING_STR = "WARNING";
        public const int LOGLEVEL_INFO = 3;
        private const string LOGLEVEL_INFO_STR = "INFO";


        public static void Init()
        {
            FileInfo f = new FileInfo(LOGFILE);
            if (f.Exists && f.Length > MAXSIZE)
            {
                var timestamp = DateTime.Now.ToString("_yyyyMMdd_HHmm");
                var moveTo = LOGFILE_STEM + @timestamp + LOGFILE_EXT;
                File.Move(LOGFILE, moveTo);
            }
        }

        public static void Log(int logLevel, string message)
        {
            try
            {
                string logLine = DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff");

                switch (logLevel)
                {
                    case LOGLEVEL_ERROR:
                        logLine += " " + LOGLEVEL_ERROR_STR + " ";
                        break;
                    case LOGLEVEL_WARNING:
                        logLine += " " + LOGLEVEL_WARNING_STR + " ";
                        break;
                    case LOGLEVEL_INFO:
                        logLine += " " + LOGLEVEL_INFO_STR + " ";
                        break;
                }

                logLine += message;

                using (StreamWriter w = File.AppendText(LOGFILE))
                {
                    w.WriteLine(logLine);
                }
            }
            catch (Exception ex) { }
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

    }
}