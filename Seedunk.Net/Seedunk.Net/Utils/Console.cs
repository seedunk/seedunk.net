using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;


namespace Seedunk.Net.Utils
{



    public delegate bool ConsoleReadListener(char key, string text);
    /// <summary>
    /// 控制台帮助类
    /// </summary>
    public partial class Console
    {
        public static void WriteLine(string text)
        {
            Log.i("", text);
        }

        public static void WriteLine(string tag, string text)
        {
            Log.i(tag, text);
        }
        public static string ReadLine(string message)
        {

            Log.i(message, "", false);
            System.Console.SetCursorPosition(System.Console.CursorLeft, System.Console.CursorTop);
            return System.Console.ReadLine();
        }
        public static char ReadKey(string message)
        {

            Log.i(message, "", false);
            System.Console.SetCursorPosition(System.Console.CursorLeft, System.Console.CursorTop);
            return System.Console.ReadKey().KeyChar;
        }
        public static void IsReadKey(string message, ConsoleReadListener listener)
        {

            Log.i(message, "", false);
            System.Console.SetCursorPosition(System.Console.CursorLeft, System.Console.CursorTop);
            string text = "";
            char key;
            do
            {
                ConsoleKeyInfo info = System.Console.ReadKey();
                if (info.KeyChar == (char)13)
                {
                    break;
                }
                key = info.KeyChar;
            } while (!listener(key, text));



        }



    }
}
