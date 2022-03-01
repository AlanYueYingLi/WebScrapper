using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace DevTool.Module.Comm.Interface
{
    public class SerialComm : CommSM
    {
        // Constant Field
        private const int _recvPeriod = 100; // Unit: mSec

        // Private Field
        private SerialPort _serialPort;
        private Dictionary<string, string> _setting;

        // Public Property
        public string SerialPortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }


        public SerialComm(string name) : base(name)
        {
            // Update _setting
            _setting = Setting;
        }

        ~SerialComm()
        {
            Dispose();
        }

        private void Dispose()
        {
            //// Write back ini file
            //Setting = _setting;
            //WriteIniSetting();
        }

        #region Protected Override Method

        protected override void DoInit()
        {
            base.DoInit();

            // Initialize the SerialPort
            _serialPort = new SerialPort();
        }

        protected override void DoActivate()
        {
            base.DoActivate();

            // Config the appropriate properties.
            _serialPort.PortName = SerialPortName;
            _serialPort.BaudRate = BaudRate;
            _serialPort.Parity = Parity;
            _serialPort.DataBits = DataBits;
            _serialPort.StopBits = StopBits;
            // serialPort.Handshake;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = ReadTimeout;
            _serialPort.WriteTimeout = WriteTimeout;

            // Open the port
            _serialPort.Open();
        }

        protected override void DoDeactivate()
        {
            base.DoDeactivate();

            // Close the port
            _serialPort.Close();
        }

        protected override void DoExit()
        {
            base.DoExit();

            // Dispose the SerialPort
            _serialPort.Dispose();
        }

        protected override void DoAlarm(Exception ex)
        {
            base.DoAlarm(ex);
        }

        protected override void DoReset()
        {
            base.DoReset();
        }
        
        #endregion

        #region Public Method

        public void Send(string buf)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                _serialPort.Write(buf);
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public void Send(byte[] buf)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                _serialPort.Write(buf, 0, buf.Length);
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public string ReceiveString(int timeoutMSec = 1000)
        {
            int i = 0;
            int recvCount;
            if (timeoutMSec <= 0)
                recvCount = 1;  // At least 1 loop
            else
                recvCount = timeoutMSec / _recvPeriod;
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                while (i < recvCount)
                {
                    string result = _serialPort.ReadExisting();
                    if (result.Length == 0)
                    {
                        // Wait for next round Read
                        Thread.Sleep(_recvPeriod);
                        continue;
                    }
                    return result;
                }
                throw handleException(encapException(string.Format("Timeout = {0}ms.", timeoutMSec)));
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public byte[] ReceiveByte(int timeoutMSec = 1000)
        {
            int i = 0;
            int recvCount;
            if (timeoutMSec <= 0)
                recvCount = 1;  // At least 1 loop
            else
                recvCount = timeoutMSec / _recvPeriod;
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                while (i < recvCount)
                {
                    int length = _serialPort.BytesToRead;
                    if (length == 0)
                    {
                        // Wait for next round Read
                        Thread.Sleep(_recvPeriod);
                        i++;
                        continue;
                    }
                    byte[] buf = new byte[length];
                    _serialPort.Read(buf, 0, length);
                    return buf;
                }
                throw handleException(encapException(string.Format("Timeout = {0}ms.", timeoutMSec)));
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        #endregion

        #region Private Method

        #endregion
    }
}
