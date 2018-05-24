using DragonExMiningSampleCSharp.Apis.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static DragonExMiningSampleCSharp.Apis.Common.Enums;

namespace DragonExMiningSampleCSharp.Apis.Log
{
    class LogTool
    {
        /// <summary>
        /// Lock object for multi thread logging
        /// </summary>
        public static object lockObj = new object();

        /// <summary>
        /// Log path
        /// </summary>
        public static string LogPath = Properties.Settings.Default.LogPath;

        /// <summary>
        /// Log levels
        /// </summary>
        public static string LogLevels = Properties.Settings.Default.LogLevels;

        /// <summary>
        /// Output log for api messages
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="api"></param>
        /// <param name="logLevel"></param>
        public static void Log(string msg, string api, LogLevels logLevel)
        {
            if (!LogLevels.Contains(logLevel.ToString()))
            {
                return;
            }
            var currentDate = DateTime.Now;
            string path = LogPath + "\\" + api + "\\" + currentDate.ToString("yyyyMMdd");
            string file = path + "\\" + currentDate.ToString(Constants.LOG_FILE_DATE_FORMAT) + ".log";
            lock (lockObj)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var txw = new StreamWriter(fs))
                    {
                        txw.Write(currentDate.ToString(Constants.LOG_DATE_FORMAT) + " " + logLevel.ToString() + " ");
                        txw.WriteLine(msg);
                    }
                }
            }
        }

        /// <summary>
        /// Log for http
        /// </summary>
        /// <param name="api"></param>
        /// <param name="msg"></param>
        /// <param name="baseAddress"></param>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <param name="code"></param>
        /// <param name="response"></param>
        public static void LogHttp(string api, string msg, Uri baseAddress, Uri path, 
            string method, HttpStatusCode code, string response)
        {
            string finalMessage = string.Format("ApiName:{0}\r\nMessage:{1}\r\nAccess Address:{2}\r\nMethod:{3}\r\nStatus Code:{4}\r\nResponse:{5}\r\n",
                api, msg, baseAddress.AbsolutePath + path.AbsolutePath, method, code.ToString(), response);
        }

        /// <summary>
        /// Output log for trade info
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="logLevel">Log level</param>
        public static void LogTradeInfo(string msg, LogLevels logLevel)
        {
            if (!LogLevels.Contains(logLevel.ToString()))
            {
                return;
            }
            var currentDate = DateTime.Now;
            string path = LogPath + "\\Trade\\" + currentDate.ToString("yyyyMMdd");
            string file = path + "\\" + currentDate.ToString(Constants.TRADE_LOG_FILE_DATE_FORMAT) + ".log";
            lock (lockObj)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var txw = new StreamWriter(fs))
                    {
                        txw.Write(currentDate.ToString(Constants.LOG_DATE_FORMAT) + " " + logLevel.ToString() + " ");
                        txw.WriteLine(msg);
                    }
                }
            }
        }
    }
}
