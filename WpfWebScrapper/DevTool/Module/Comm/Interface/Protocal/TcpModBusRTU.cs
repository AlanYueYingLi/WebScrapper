using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool.Module.Comm.Interface.Protocal
{
    public class TcpModBusRTU : TcpClientApp
    {
        // ModBus RTU protocol
        // Private Constant Field
        private const byte _readCoil = 0x01;    //Read Coils
        private const byte _readDiscreteInput = 0x02;   //Read Discrete Inputs
        private const byte _readHoldingReg = 0x03; //Read Holding Registers
        private const byte _readInputReg = 0x04;    //Read Input Registers
        private const byte _writeSignleCoil = 0x05; //Write Single Coil
        private const byte _writeSingleReg = 0x06;   //Write Single Register
        private const byte _writeMultiReg = 0x10;    //Write Multiple Registers

        // Private Field
        private Dictionary<string, string> _setting;
        // ModBus TCP protocol
        private ushort _uTransID = 0;  // Counter for Transaction ID
        private byte[] _transID = new byte[2];  // Transaction ID
        private byte[] _protocolID = { 0x00, 0x00 };    //Protocol ID which Modbus TCP = 0
        private byte[] _transLength = { 0x00, 0x06 };   //Transaction Length

        // Public Property
        public byte SlaveAddress { get; set; } //ModBus RTU Slave address

        public enum CmdType
        {
            ReadCoil = _readCoil,
            ReadDiscreteInput = _readDiscreteInput,
            ReadHoldingRegister = _readHoldingReg,
            ReadInputRegister = _readInputReg,
            WriteSingleCoil = _writeSignleCoil,
            WriteSingleRegister = _writeSingleReg,
            WriteMultiRegister = _writeMultiReg
        }

        public TcpModBusRTU(string name) : base(name)
        {
            // Update _setting
            _setting = Setting;
        }

        ~TcpModBusRTU()
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

        #endregion

        #region Public Static Method

        public static bool[] ConvertByteToBoolArray(byte b)
        {
            // Prepare the return result
            bool[] result = new bool[8];

            // Check each bit in the byte. If 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            return result;
        }

        #endregion

        #region Public Method

        public void Read(byte[] regAddr, CmdType cmd, out ushort byteCount, out byte[] byteData, ushort dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // Build the payload
                byte[] package = modTcp_Encode_Read(regAddr, cmd, dataCount);

                // Read/Write via TcpClient
                this.Send(package);
                byte[] recvPackage = this.ReceiveByte();

                // Parse the payload
                modTcp_Decode_Read(recvPackage, cmd, out byteCount, out byteData);

                // Counter roll-up
                _uTransID++;
            }
            catch (Exception ex)
            {
                byteCount = new ushort();
                byteData = new byte[0];
                throw handleException(encapException(ex));
            }
        }

        public void Write(byte[] regAddr, CmdType cmd, ushort[] data, ushort dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // Build the payload
                byte[] package = modTcp_Encode_Write(regAddr, cmd, data, dataCount);

                // Read/Write via TcpClient
                this.Send(package);
                byte[] recvPackage = this.ReceiveByte();

                // Parse the payload
                modTcp_Decode_Write(recvPackage, cmd);

                // Counter roll-up
                _uTransID++;
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        /// <summary>
        /// Evaluate MODBUS RTU CRC
        /// </summary>
        public byte[] ModTcp_CRC(byte[] buf)
        {
            ushort u16CRC = 0xFFFF;
            int len = buf.Length;
            for (int pos = 0; pos < len; pos++)
            {
                u16CRC ^= (ushort)buf[pos];          // 取出第一個byte與crc XOR

                for (int i = 8; i != 0; i--)
                {    // 巡檢每個bit  
                    if ((u16CRC & 0x0001) != 0)
                    {      // 如果 LSB = 1   
                        u16CRC >>= 1;                    // 右移1bit 並且 XOR 0xA001  
                        u16CRC ^= 0xA001;
                    }
                    else                            // 如果 LSB != 1  
                        u16CRC >>= 1;                    // 右移1bit
                }
            }
            byte[] crc = BitConverter.GetBytes(u16CRC);
            return crc;
        }

        #endregion

        #region Private Method

        private byte[] modTcp_Encode_Read(byte[] regAddr, CmdType cmd, ushort dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.WriteSingleCoil || cmd == CmdType.WriteMultiRegister || cmd == CmdType.WriteSingleRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                List<byte> listDataFrame = new List<byte>();

                //Transaction ID
                _transID = BitConverter.GetBytes(_uTransID);
                Array.Reverse(_transID);
                foreach (var item in _transID)
                    listDataFrame.Add(item);
                //Protocol ID
                foreach (var item in _protocolID)
                    listDataFrame.Add(item);
                //Transaction Length
                foreach (var item in _transLength)
                    listDataFrame.Add(item);

                //ID number
                listDataFrame.Add(SlaveAddress);
                //Command
                listDataFrame.Add((byte)cmd);
                //Register Address
                foreach (byte item in regAddr)
                    listDataFrame.Add(item);
                //Data Count
                byte[] bDataCount = BitConverter.GetBytes(dataCount);
                // Reverse the byte order since BitConverter get Little Endian but we need Big Endian here
                Array.Reverse(bDataCount);
                foreach (byte item in bDataCount)
                    listDataFrame.Add(item);

                return listDataFrame.ToArray();
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        private byte[] modTcp_Encode_Write(byte[] regAddr, CmdType cmd, ushort[] data, ushort dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.ReadCoil || cmd == CmdType.ReadDiscreteInput || cmd == CmdType.ReadInputRegister || cmd == CmdType.ReadHoldingRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                List<byte> listDataFrame = new List<byte>();

                //Transaction ID
                _transID = BitConverter.GetBytes(_uTransID);
                Array.Reverse(_transID);
                foreach (var item in _transID)
                    listDataFrame.Add(item);
                //Protocol ID
                foreach (var item in _protocolID)
                    listDataFrame.Add(item);
                //Transaction Length
                foreach (var item in _transLength)
                    listDataFrame.Add(item);

                //ID number
                listDataFrame.Add(SlaveAddress);
                //Command
                listDataFrame.Add((byte)cmd);
                //Register Address
                foreach (byte item in regAddr)
                    listDataFrame.Add(item);

                if (cmd == CmdType.WriteMultiRegister)
                {
                    // Add DataCount & ByteCount
                    //Data Count
                    byte[] bDataCount = BitConverter.GetBytes(dataCount);
                    // Reverse the byte order since BitConverter get Little Endian but we need Big Endian here
                    Array.Reverse(bDataCount);
                    foreach (byte item in bDataCount)
                        listDataFrame.Add(item);
                    //Byte Count
                    byte bByteCount = byte.Parse((2 * dataCount).ToString());
                    listDataFrame.Add(bByteCount);
                }
                //Data
                foreach (ushort item in data)
                {
                    byte[] temp = BitConverter.GetBytes(item);
                    // Big Endian
                    foreach (var subItem in temp)
                        listDataFrame.Add(subItem);
                }

                return listDataFrame.ToArray();
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        private void modTcp_Decode_Read(byte[] buf, CmdType cmd, out ushort byteCount, out byte[] data)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.WriteSingleCoil || cmd == CmdType.WriteMultiRegister || cmd == CmdType.WriteSingleRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                //Transaction ID
                byte[] recvTransID = { buf[1], buf[0] };
                ushort uRecvTransID = BitConverter.ToUInt16(recvTransID, 0);
                if (_uTransID != uRecvTransID)
                    throw new Exception("Invalid Transaction ID = " + uRecvTransID + ", which shall be = " + _uTransID);
                //Protocol ID
                byte[] recvProtocolID = { buf[3], buf[2] };
                ushort uRecvProtocolID = BitConverter.ToUInt16(recvProtocolID, 0);
                ushort protocolID = BitConverter.ToUInt16(_protocolID, 0);
                if (protocolID != uRecvProtocolID)
                    throw new Exception("Invalid Protocol ID = " + uRecvProtocolID + ", which shall be = " + protocolID);
                //Transaction Length
                byte[] recvLength = { buf[5], buf[4] };
                ushort uRecvLength = BitConverter.ToUInt16(recvLength, 0);
                byte[] buffer = new byte[uRecvLength];
                Array.Copy(buf, 6, buffer, 0, uRecvLength);

                // Check buffer length
                if (buffer.Length < 3)
                    throw new Exception("Buffer too short! The length is " + buffer.Length);
                // Check Address
                if (buffer[0] != SlaveAddress)
                    throw new Exception("Invalid ID address!");
                // Check CmdType
                if (buffer[1] != (byte)cmd)
                    throw new Exception("Unexpected Cmd!\n" + buffer[1].ToString());
                // Get the byte count
                byteCount = (ushort)buffer[2];
                data = new byte[byteCount];
                // Parse data from buffer
                Buffer.BlockCopy(buffer, 3, data, 0, byteCount);
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        private void modTcp_Decode_Write(byte[] buf, CmdType cmd)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.ReadCoil || cmd == CmdType.ReadDiscreteInput || cmd == CmdType.ReadInputRegister || cmd == CmdType.ReadHoldingRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                //Transaction ID
                byte[] recvTransID = { buf[1], buf[0] };
                ushort uRecvTransID = BitConverter.ToUInt16(recvTransID, 0);
                if (_uTransID != uRecvTransID)
                    throw new Exception("Invalid Transaction ID = " + uRecvTransID + ", which shall be = " + _uTransID);
                //Protocol ID
                byte[] recvProtocolID = { buf[3], buf[2] };
                ushort uRecvProtocolID = BitConverter.ToUInt16(recvProtocolID, 0);
                ushort protocolID = BitConverter.ToUInt16(_protocolID, 0);
                if (protocolID != uRecvProtocolID)
                    throw new Exception("Invalid Protocol ID = " + uRecvProtocolID + ", which shall be = " + protocolID);
                //Transaction Length
                byte[] recvLength = { buf[5], buf[4] };
                ushort uRecvLength = BitConverter.ToUInt16(recvLength, 0);
                byte[] buffer = new byte[uRecvLength];
                Array.Copy(buf, 6, buffer, 0, uRecvLength);

                // Check buffer length
                if (buffer.Length < 2)
                    throw new Exception("Buffer too short! The length is " + buffer.Length);
                // Check Address
                if (buffer[0] != SlaveAddress)
                    throw new Exception("Invalid ID address!");
                // Check CmdType
                if (buffer[1] != (byte)cmd)
                    throw new Exception("Unexpected Cmd! " + buffer[1].ToString());
            }
            catch (Exception ex)
            {
                throw handleException(encapException(ex));
            }
        }

        #endregion
    }

}
