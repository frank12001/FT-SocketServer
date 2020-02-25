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
        private static Printer Instance => _instance ?? (_instance = new Printer());

        /// <summary>
        /// String builder for cache
        /// </summary>
        private readonly StringBuilder _builder;
        /// <summary>
        /// Timer that print strings automatically
        /// </summary>
        private Timer printTimer;
        /// <summary>
        /// Show date time
        /// </summary>
        private bool _showTime;
        /// <summary>
        /// Date time format
        /// </summary>
        private string _timeFormat;
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
            _builder = new StringBuilder();
            printTimer = new Timer(100);
            printTimer.Elapsed += Print;
            printTimer.Start();

            _showTime = false;
            _timeFormat = "yyyy-MM-dd HH:mm:ss";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Write without new line
        /// </summary>
        /// <param name="str">string</param>
        public static void Write(object str)
        {
            lock (Instance._builder)
            {
                Instance._builder.Append(str);
            }
        }

        /// <summary>
        /// Write with new line
        /// </summary>
        /// <param name="str">string</param>
        public static void WriteLine(object str)
        {
            lock (Instance._builder)
            {
                if (Instance._showTime)
                {
                    Instance._builder.Append("[" + DateTime.Now.ToString(Instance._timeFormat) + "] ");
                    Instance._builder.Append(str);
                    Instance._builder.Append("\n");
                }
                else
                {
                    Instance._builder.Append(str + "\n");
                }
            }
        }

        public static void WriteWarning(object str)
        {
            lock (Instance._builder)
            {
                if (Instance._builder.Length > 0)
                {
                    string temp = Instance._builder.ToString();
                    Console.Write(temp);
                    Instance._builder.Clear();
                }

                Console.Write("[" + DateTime.Now.ToString(Instance._timeFormat) + "] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str);
                Console.ResetColor();
            }
        }

        public static void WriteError(object str)
        {
            lock (Instance._builder)
            {
                if (Instance._builder.Length > 0)
                {
                    string temp = Instance._builder.ToString();
                    Console.Write(temp);
                    Instance._builder.Clear();
                }

                Console.Write("[" + DateTime.Now.ToString(Instance._timeFormat) + "] ");
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
            Instance._showTime = show;
            Instance._timeFormat = format;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Print strings
        /// </summary>
        private void Print(object obj, ElapsedEventArgs args)
        {
            lock (_builder)
            {
                if (_builder.Length > 0)
                {
                    string str = _builder.ToString();
                    Console.Write(str);
                    _builder.Clear();
                }
            }
        }
        #endregion
    }
}
