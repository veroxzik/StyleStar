using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class Logger
    {
        public static string LogFilename;

        public static void WriteEntry(string entry)
        {
            if (LogFilename == null)
                LogFilename = "StyleStarLog-" + GetDateTimeString() + ".txt";

            using (StreamWriter sw = new StreamWriter(new FileStream(LogFilename, FileMode.Append)))
            {
                sw.WriteLine(GetDateTimeString() + ": " + entry);
            }
        }

        public static string GetDateTimeString()
        {
            return DateTime.Now.ToString("yyyyMMdd HHmm");
        }
    }
}
