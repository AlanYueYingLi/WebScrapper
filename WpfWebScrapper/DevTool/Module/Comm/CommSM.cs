using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DevTool.File;

namespace DevTool.Module.Comm
{
    public class CommSM : StateMachineBase
    {
        // Public Property
        public string Version { get; }
        public bool IsLog { get; } = false;
        public Dictionary<string, string> Setting
        {
            get { return _iniDicRead; }
            set { _iniDicWrite = value; }
        }
        public string CurrentDirPath { get { return _iniHandle.CurrentDirPath; } }
        public string IniDirPath { get { return _iniHandle.IniDirPath; } }
        public int CmdCount { get { return _inQueue.Count; } }

        // Private Field
        private string _name;
        private IniFileHandler _iniHandle;
        private LogHandler _logHandle;
        private Dictionary<string, string> _iniDicRead = new Dictionary<string, string>();
        private Dictionary<string, string> _iniDicWrite = new Dictionary<string, string>();
        private Queue<object[]> _inQueue = new Queue<object[]>();

        // Protected Property
        //protected Exception Exception { get; set; }
        
        public CommSM(string name)
        {
            try
            {
                // Assign name
                _name = name;
                
                // Create a LogHandler to save log
                _logHandle = new LogHandler(_name);

                // Create a IniFileHandler to get setting
                _iniHandle = new IniFileHandler(_name);

                // Get Ini setting
                readIniDic();
                // Update properties
                if (bool.Parse(_iniDicRead["IsLog"]))
                    IsLog = true;
                Version = _iniDicRead["Version"];
            }
            catch (Exception ex)
            {
                // Do Log
                Log("Exception", ex.ToString());
                throw ex;
            }
        }

        ~CommSM()
        {
            // Secure that the communication has been deactivated or closed
            if (CurrentState == ModuleState.Run)
                End();
            if (CurrentState == ModuleState.Active)
                Deactivate();
            if (CurrentState == ModuleState.Inactive)
                Exit();
        }

        #region Public Methods

        public void Log(string obj, string msg)
        {
            if (IsLog)
            {
                var time = DateTime.Now;
                // Write to Log list
                _logHandle.WriteLog(obj, msg, time);
            }
            
            // Print to Console if in Debug mode
            debugLog(string.Format("{0}\t{1}", obj, msg));
        }

        public void WriteIniSetting()
        {
            try
            {
                _iniHandle.WriteIni(_iniDicWrite);
                // Update local variable
                readIniDic();
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void WriteCmd(object cmd)
        {
            var queue = new object[2];
            queue[0] = cmd;
            queue[1] = null;
            _inQueue.Enqueue(queue);
        }

        public void WriteCmd(object cmd, object param)
        {
            var queue = new object[2];
            queue[0] = cmd;
            queue[1] = param;
            _inQueue.Enqueue(queue);
        }

        public object GetCmd()
        {
            var queue = _inQueue.Dequeue();
            return queue[0];
        }

        public object GetCmd(out object param)
        {
            var queue = _inQueue.Dequeue();
            param = queue[1];
            return queue[0];
        }

        // State Control Methods
        public void Init()
        {
            try
            {
                DoInit();
                MoveNext(StateCommand.Init);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void Activate() 
        {
            try
            {
                DoActivate();
                MoveNext(StateCommand.Activate);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void Deactivate()
        {
            try
            {
                DoDeactivate();
                MoveNext(StateCommand.Deactivate);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void Exit()
        {
            try
            {
                DoExit();
                MoveNext(StateCommand.Exit);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void Alarm(Exception ex)
        {
            try
            {
                // Do Log
                Log("Exception", ex.ToString());

                DoAlarm(ex);
                MoveNext(StateCommand.Alarm);
            }
            catch { }
        }

        public void Reset()
        {
            try
            {
                // Do Log
                Log("Exception", "Reset!");

                DoReset();
                MoveNext(StateCommand.Reset);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void Begin()
        {
            try
            {
                MoveNext(StateCommand.Begin);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void End()
        {
            try
            {
                MoveNext(StateCommand.End);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        #endregion

        #region Private Methods

        private void readIniDic()
        {
            try
            {
                _iniDicRead = _iniHandle.ReadIni();
                // Update local variable
                _iniDicWrite = _iniDicRead;
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        [Conditional("DEBUG")]
        private void debugLog(string msg)
        {
            Console.WriteLine(msg);
        }

        #endregion

        #region Protected Method

        protected Exception handleException(Exception ex)
        {
            // Switch state to Error state
            Alarm(ex);
            //// Update property
            //Exception = ex;
            //// Throw Exception
            //throw ex;
            return ex;
        }

        protected Exception encapException(Exception ex)
        {
            // Get calling method name
            string caller = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;

            // Encapsulate new Exception
            Exception exception = new Exception(string.Format("{0} Exception!{1}{2}", caller, System.Environment.NewLine, ex.ToString()));
            return exception;
        }

        protected Exception encapException(String description)
        {
            // Get calling method name
            string caller = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;

            // Encapsulate new Exception
            Exception exception = new Exception(string.Format("{0} Exception!{1}{2}", caller, System.Environment.NewLine, description));
            return exception;
        }

        #endregion

        #region Protected Virtual Method for Override

        protected virtual void DoInit() { }

        protected virtual void DoActivate() { }

        protected virtual void DoDeactivate() { }

        protected virtual void DoExit() { }

        protected virtual void DoAlarm(Exception ex) { }

        protected virtual void DoReset() { }

        #endregion
    }
}
