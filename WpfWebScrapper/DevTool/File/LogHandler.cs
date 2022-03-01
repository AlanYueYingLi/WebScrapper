using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevTool.File
{
    public class LogHandler
    {
        // Constant Field
        private const int _logInterval = 1000; //Unit: ms
        private const string _logFormat = ".txt";
        
        // Private Field
        private string _name;
        private string _logDirPath = Environment.CurrentDirectory + "\\Log";
        private string _filePath;
        private List<LogMsg> _logMsgList;
        private System.Timers.Timer _timer;

        private struct LogMsg
        {
            public string Object;
            public string Message;
            public DateTime Time;
        }

        // Public Property
        public string FilePath
        {
            get { return _filePath; }
            private set { _filePath = value; }
        }

        public LogHandler(string name)
        {
            // Assign name
            _name = name;

            // Create Log directory if not exist
            if (!Directory.Exists(_logDirPath))
                Directory.CreateDirectory(_logDirPath);

            // Assign the File path
            //FilePath = Path.Combine(_logDirPath, _name + _logFormat);

            // Initialize Fields
            _logMsgList = new List<LogMsg>();

            // Create and Start timer
            _timer = new System.Timers.Timer();
            _timer.Interval = _logInterval;
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        ~LogHandler()
        {
            if (_timer != null)
                _timer.Stop();
        }

        #region Timer Handler

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_logMsgList)
            {
                if (_logMsgList.Count != 0)
                {
                    // Write into File
                    writeIntoFile(_logMsgList);

                    // Clean the list
                    _logMsgList.Clear();
                }
            }
        }

        #endregion

        #region Public Methods

        public void WriteLog(string obj, string msg, DateTime time)
        {
            LogMsg logMsg = new LogMsg() { Object = obj, Message = msg, Time = time };
            lock (_logMsgList)
            {
                _logMsgList.Add(logMsg);
            }
        }

        #endregion

        #region Private Methods

        private void writeIntoFile(List<LogMsg> logMsgs)
        {
            int numberOfRetries = 3;
            int delayOnRetry = 100;
            
            for (int i = 0; i < numberOfRetries; i++)
            {
                try
                {
                    // Create the file by Now data time
                    var dateTime = DateTime.Now;
                    var stringDate = dateTime.ToString("yyyyMMdd");
                    var fileName = string.Format("{0}{1}", stringDate, _logFormat);
                    var folderPath = Path.Combine(_logDirPath, _name);
                    var filePath = Path.Combine(folderPath, fileName);
                    Directory.CreateDirectory(folderPath);

                    // Append the message if the file existed
                    using (StreamWriter outputFile = new StreamWriter(filePath, true))
                    {
                        foreach (var logMsg in logMsgs)
                        {
                            string log = string.Format("{0}\t{1}\t{2}", logMsg.Time.ToString("HH:mm:ss.fff"), logMsg.Object, logMsg.Message);
                            outputFile.WriteLine(log);
                        }
                    }
                    return;
                }
                catch (IOException) when (i < numberOfRetries)
                {
                    Thread.Sleep(delayOnRetry);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion
    }
}
