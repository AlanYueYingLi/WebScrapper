using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace DevTool.Module.Comm.Interface
{
    public class TcpClientApp : CommSM
    {
        // Private Field
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private string _hostName;
        private int _port;
        private bool _isConnectionSuccess = false;
        private bool _isSendSuccess = false;
        private bool _isRecvSuccess = false;
        private byte[] _recvBuffer;
        private Exception _tcpClientAppException;
        private ManualResetEvent _timeoutObject = new ManualResetEvent(false);
        private Dictionary<string, string> _setting;

        // Public Property
        public string HostName { get; set; }
        public int Port { get; set; }
        public int TimeoutMSec { get; set; }


        public TcpClientApp(string name) : base(name)
        {
            // Update _setting
            _setting = Setting;           
        }

        ~TcpClientApp()
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

            // Create new TcpClient instance for new connection
            _tcpClient = new TcpClient();
        }

        protected override void DoActivate()
        {
            base.DoActivate();

            _hostName = HostName;
            _port = Port;
            _isConnectionSuccess = false;
            _timeoutObject.Reset();
            _tcpClientAppException = null;

            _tcpClient.BeginConnect(_hostName, _port, new AsyncCallback(CB_BeginConnect), _tcpClient);
            // Wait connection call-back
            if (_timeoutObject.WaitOne(TimeoutMSec, false))
            {
                if (!_isConnectionSuccess)
                    throw _tcpClientAppException;

                _networkStream = _tcpClient.GetStream();
                _recvBuffer = new byte[_tcpClient.ReceiveBufferSize];
            }
            else
            {
                _tcpClient.Close();
                throw new TimeoutException("TcpClient BeginConnect Timeout Exception");
            }
        }

        protected override void DoDeactivate()
        {
            base.DoDeactivate();

            _tcpClient.Close();
        }

        protected override void DoExit()
        {
            base.DoExit();

            _tcpClient.Dispose();
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
        public void Send(string buffer, int timeoutMSec = 1000)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                byte[] bBuffer = Encoding.ASCII.GetBytes(buffer);
                _isSendSuccess = false;
                _timeoutObject.Reset();
                _tcpClientAppException = null;

                _networkStream.BeginWrite(bBuffer, 0, bBuffer.Length, new AsyncCallback(CB_BeginWrite), null);
                // Wait write call-back
                if (_timeoutObject.WaitOne(timeoutMSec, false))
                {
                    if (!_isSendSuccess)
                        throw _tcpClientAppException;
                }
                else
                {
                    throw new TimeoutException("NetworkStream BeginWrite Timeout Exception");
                }
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public void Send(byte[] buffer, int timeoutMSec = 1000)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                _isSendSuccess = false;
                _timeoutObject.Reset();
                _tcpClientAppException = null;

                _networkStream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(CB_BeginWrite), null);
                // Wait write call-back
                if (_timeoutObject.WaitOne(timeoutMSec, false))
                {
                    if (!_isSendSuccess)
                        throw _tcpClientAppException;
                }
                else
                {
                    throw new TimeoutException("NetworkStream BeginWrite Timeout Exception");
                }
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public string ReceiveString(int timeoutMSec = 1000)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                _isRecvSuccess = false;
                _timeoutObject.Reset();
                _tcpClientAppException = null;

                _networkStream.BeginRead(_recvBuffer, 0, _recvBuffer.Length, new AsyncCallback(CB_BeginRead), null);
                // Wait read call-back
                if (_timeoutObject.WaitOne(timeoutMSec, false))
                {
                    if (_isRecvSuccess)
                    {
                        string stream = Encoding.ASCII.GetString(_recvBuffer);
                        return stream;
                    }
                    else
                    {
                        throw _tcpClientAppException;
                    }
                }
                else
                {
                    throw new TimeoutException("NetworkStream BeginRead Timeout Exception");
                }
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        public byte[] ReceiveByte(int timeoutMSec = 1000)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                _isRecvSuccess = false;
                _timeoutObject.Reset();
                _tcpClientAppException = null;

                _networkStream.BeginRead(_recvBuffer, 0, _recvBuffer.Length, new AsyncCallback(CB_BeginRead), null);
                // Wait read call-back
                if (_timeoutObject.WaitOne(timeoutMSec, false))
                {
                    if (_isRecvSuccess)
                    {
                        string stream = Encoding.ASCII.GetString(_recvBuffer);
                        return _recvBuffer;
                    }
                    else
                    {
                        throw _tcpClientAppException;
                    }
                }
                else
                {
                    throw new TimeoutException("NetworkStream BeginRead Timeout Exception");
                }
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        #endregion

        #region Private Method

        #endregion

        #region Call-Back Method
        private void CB_BeginConnect(IAsyncResult asyncResult)
        {
            try
            {
                if (_tcpClient.Client != null)
                {
                    _tcpClient.EndConnect(asyncResult);
                    _isConnectionSuccess = true;
                }
            }
            catch (Exception ex)
            {
                _tcpClientAppException = ex;
            }
            finally
            {
                _timeoutObject.Set();
            }
        }

        private void CB_BeginWrite(IAsyncResult asyncResult)
        {
            try
            {
                _networkStream.EndWrite(asyncResult);
                _isSendSuccess = true;
            }
            catch (Exception ex)
            {
                _tcpClientAppException = ex;
            }
            finally
            {
                _timeoutObject.Set();
            }
        }

        private void CB_BeginRead(IAsyncResult asyncResult)
        {
            try
            {
                _networkStream.EndRead(asyncResult);
                _isRecvSuccess = true;
            }
            catch (Exception ex)
            {
                _tcpClientAppException = ex;
            }
            finally
            {
                _timeoutObject.Set();
            }
        }

        #endregion
    }
}
