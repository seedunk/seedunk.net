using System;
using System.Text.RegularExpressions;

namespace Seedunk.Net.Utils
{


    public class Log
    {
        public static void i(string log)
        {
            i(" ", log);
        }

        public static int getTestLength(string text)
        {
            int len = 1;
            for (int i = 0; i < text.Length; i++)
            {
                Regex rx = new Regex("^[\u4e00-\u9fa5]$");
                len += rx.IsMatch(text.Substring(i, 1)) ? 2 : 1;
            }
            return len;

        }

        public static string Format(string tag, string log)
        {
            int rightLength = 20;
            for (int i = 0; i < tag.Length; i++)
            {
                Regex rx = new Regex("^[\u4e00-\u9fa5]$");
                if (rx.IsMatch(tag.Substring(i, 1)))
                {
                    rightLength = rightLength - 1;
                }
            }
            return string.Format("{0} | {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tag.PadLeft(rightLength, ' '), log);

        }
        public static void d(string tag, string log, bool allLine = true)
        {
            string line = Format(tag, log);
            if (allLine)
            {
                System.Console.WriteLine(line);
            }
            else
            {
                System.Console.Write(line);
            }
        }

        public static void i(string tag, string log, CallBack<string> callBack)
        {
            string line = Format(tag, log);
            System.Console.WriteLine(line);
            callBack(line);
        }
        public static void i(string tag, string log, bool allLine = true)
        {
            string line = Format(tag, log);
            if (allLine)
            {
                System.Console.WriteLine(line);
            }
            else
            {
                System.Console.Write(line);
            }
        }

    }
}
