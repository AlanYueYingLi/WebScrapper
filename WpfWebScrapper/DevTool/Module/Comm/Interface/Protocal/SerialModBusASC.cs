using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool.Module.Comm.Interface.Protocal
{
	public class SerialModBusASC : SerialComm
	{
        // Private Constant Field
        // ModBus RTU protocol
        private const byte _readCoil = 0x01;    //Read Coils
        private const byte _readInput = 0x02;   //Read Discrete Inputs
        private const byte _readHoldReg = 0x03; //Read Holding Registers
        private const byte _readInputReg = 0x04;    //Read Input Registers
        private const byte _writeSignleCoil = 0x05; //Write Single Coil
        private const byte _writeSingleReg = 0x06;   //Write Single Register
        private const byte _writeMultiReg = 0x10;    //Write Multiple Registers

        // Public Property
        public byte SlaveAddress { get; set; } //ModBus RTU Slave address

        public enum CmdType
        {
            ReadCoil = _readCoil,
            ReadInput = _readInput,
            ReadHoldRegister = _readHoldReg,
            ReadInputRegister = _readInputReg,
            WriteCoil = _writeSignleCoil,
            WriteSingleRegister = _writeSingleReg,
            WriteMultiRegister = _writeMultiReg
        }

        public SerialModBusASC(string name) : base(name)
        {

        }

        ~SerialModBusASC()
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

        #region Public Method

        public byte[] ModRTU_Encode_Read(byte[] regAddr, CmdType cmd, UInt16 dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // Check cmd
                if (cmd == CmdType.WriteCoil || cmd == CmdType.WriteMultiRegister || cmd == CmdType.WriteSingleRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                // Encode
                List<byte> listDataFrame = new List<byte>();

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
                //Evaluate ModBus RTU CRC16
                byte bLRCFrame = eval_ModASC_LRC(listDataFrame.ToArray());
                listDataFrame.Add(bLRCFrame);

                return listDataFrame.ToArray();
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public byte[] ModRTU_Encode_Write(byte[] regAddr, CmdType cmd, byte[] data, UInt16 dataCount = 1)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.ReadCoil || cmd == CmdType.ReadInput || cmd == CmdType.ReadInputRegister || cmd == CmdType.ReadHoldRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                // Encode
                List<byte> listDataFrame = new List<byte>();

                int dataLength = data.Length;
                byte[] bDataFrame = new byte[6 + dataLength + 1];    //+1 because of Byte count
                byte[] bRTUMessage = new byte[8 + dataLength + 1];   //+1 because of Byte count

                //ID number
                listDataFrame.Add(SlaveAddress);
                //Command
                listDataFrame.Add((byte)cmd);
                //Register Address
                Array.Reverse(regAddr);
                foreach (byte item in regAddr)
                    listDataFrame.Add(item);
                //Data Count
                byte[] bDataCount = BitConverter.GetBytes(dataCount);
                // Reverse the byte order since BitConverter get Little Endian but we need Big Endian here
                Array.Reverse(bDataCount);
                foreach (byte item in bDataCount)
                    listDataFrame.Add(item);
                //Byte Count
                byte bByteCount = byte.Parse((2 * dataCount).ToString());
                listDataFrame.Add(bByteCount);
                //Data
                foreach (byte item in data)
                    listDataFrame.Add(item);
                //Evaluate ModBus RTU CRC16
                byte bLRCFrame = eval_ModASC_LRC(listDataFrame.ToArray());
                listDataFrame.Add(bLRCFrame);

                return listDataFrame.ToArray();
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void ModRTU_Decode_Read(byte[] buf, CmdType cmd, out int dataCount, out byte[] data)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // CmdType Check
                if (cmd == CmdType.WriteCoil || cmd == CmdType.WriteMultiRegister || cmd == CmdType.WriteSingleRegister)
                    throw new Exception(string.Format("Invalid CmdType received!{0}Cmd = {1}", System.Environment.NewLine, cmd.ToString()));

                //Decode
                int bufLength = buf.Length;
                byte[] bDataFrame = new byte[buf.Length - 2];
                Buffer.BlockCopy(buf, 0, bDataFrame, 0, bufLength - 2);
                byte bLRCFame = buf.Last();
                //CRC check
                if (!bLRCFame.Equals(eval_ModASC_LRC(bDataFrame)))
                    throw new Exception("Invalid CRC!");
                if (buf[0] != SlaveAddress)
                    throw new Exception("Invalid ID address!");
                if (buf[1] != (byte)cmd)
                    throw new Exception("Unexpected Cmd!" + System.Environment.NewLine + buf[1].ToString());

                byte byteCount = buf[2];
                dataCount = (int)byteCount / 2; //Each data costs 2 bytes
                data = new byte[(int)byteCount];
                Buffer.BlockCopy(buf, 3, data, 0, (int)byteCount);
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        public void ModRTU_Decode_Write(byte[] buf, CmdType cmd)
        {
            try
            {
                // Check state
                CheckState(ModuleState.Active);

                // Decode
                byte[] regAddr = new byte[2];

                int bufLength = buf.Length;
                byte[] bDataFrame = new byte[buf.Length - 2];
                Buffer.BlockCopy(buf, 0, bDataFrame, 0, bufLength - 2);
                byte bLRCFame = buf.Last();
                //CRC check
                if (!bLRCFame.Equals(eval_ModASC_LRC(bDataFrame)))
                    throw new Exception("Invalid CRC!");
                if (buf[0] != SlaveAddress)
                    throw new Exception("Invalid ID address!");
                if (buf[1] != (byte)cmd)
                    throw new Exception("Unexpected Cmd!" + System.Environment.NewLine + buf[1].ToString());

                Buffer.BlockCopy(buf, 2, regAddr, 0, 2);
                Array.Reverse(regAddr);
                UInt16 dataCount = (UInt16)buf[5];
            }
            catch (Exception ex)
            {
                throw handleException(ex);
            }
        }

        #endregion

        #region Private Method
        
        /// <summary>
        /// Evaluate MODBUS RTU CRC
        /// </summary>
        private byte eval_ModASC_LRC(byte[] buf)
        {
            int LRC = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                LRC -= buf[i];
            }
            return (byte)LRC;
        }

        #endregion
    }
}
