using System;
using System.Text;
using System.Timers;

namespace FTServer.Log
{
    /// <summary>
    /// Cache the strings and print automatically
    /// </summary>
    public class Printer
    {
        #region Fields
        private static Printer _instance;
        private static Printer instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Printer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// String builder for cache
        /// </summary>
        private StringBuilder builder;
        /// <summary>
        /// Timer that print strings automatically
        /// </summary>
        private Timer printTimer;
        /// <summary>
        /// Show date time
        /// </summary>
        private bool showTime;
        /// <summary>
        /// Date time format
        /// </summary>
        private string timeFormat;
        #endregion

        #region Printer Format
        private struct PrinterFormat
        {
            public ConsoleColor color;
            public string time;
            public string message;

        }
        #endregion

        #region Constructor
        private Printer()
        {
            builder = new StringBuilder();
            printTimer = new Timer(100);
            printTimer.Elapsed += Print;
            printTimer.Start();

            showTime = false;
            timeFormat = "yyyy-MM-dd HH:mm:ss";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Write without new line
        /// </summary>
        /// <param name="str">string</param>
        public static void Write(object str)
        {
            lock (instance.builder)
            {
                instance.builder.Append(str);
            }
        }

        /// <summary>
        /// Write with new line
        /// </summary>
        /// <param name="str">string</param>
        public static void WriteLine(object str)
        {
            lock (instance.builder)
            {
                if (instance.showTime)
                {
                    instance.builder.Append("[" + DateTime.Now.ToString(instance.timeFormat) + "] ");
                    instance.builder.Append(str);
                    instance.builder.Append("\n");
                }
                else
                {
                    instance.builder.Append(str + "\n");
                }
            }
        }

        public static void WriteWarning(object str)
        {
            lock (instance.builder)
            {
                if (instance.builder.Length > 0)
                {
                    string temp = instance.builder.ToString();
                    Console.Write(temp);
                    instance.builder.Clear();
                }

                Console.Write("[" + DateTime.Now.ToString(instance.timeFormat) + "] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str);
                Console.ResetColor();
            }
        }

        public static void WriteError(object str)
        {
            lock (instance.builder)
            {
                if (instance.builder.Length > 0)
                {
                    string temp = instance.builder.ToString();
                    Console.Write(temp);
                    instance.builder.Clear();
                }

                Console.Write("[" + DateTime.Now.ToString(instance.timeFormat) + "] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(str);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Show time setting
        /// </summary>
        /// <param name="show">boolean of show time</param>
        /// <param name="format">time format</param>
        public static void ShowTime(bool show, string format = "yyyy-MM-dd HH:mm:ss")
        {
            instance.showTime = show;
            instance.timeFormat = format;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Print strings
        /// </summary>
        private void Print(object obj, ElapsedEventArgs args)
        {
            lock (builder)
            {
                if (builder.Length > 0)
                {
                    string str = builder.ToString();
                    Console.Write(str);
                    builder.Clear();
                }
            }
        }
        #endregion
    }
}
