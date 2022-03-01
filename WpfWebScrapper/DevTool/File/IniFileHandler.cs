using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevTool.File
{
    public class IniFileHandler
    {
        // Public Property
        public string FilePath
        {
            get { return _filePath; }
            private set { _filePath = value; }
        }
        public string IniDirPath
        {
            get { return _iniDirPath; }
        }
        public string CurrentDirPath
        {
            get { return _currentDirPath; }
        }

        // Import DLL
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // Constant Field
        private const string _iniFormat = ".ini";

        // Private Field
        private string _name;
        private string _currentDirPath = Environment.CurrentDirectory;
        private string _iniDirPath = Environment.CurrentDirectory + "\\ini";
        private string _filePath;

        public IniFileHandler(string name)
        {
            // Assign name
            _name = name;

            // Assign the File path
            FilePath = Path.Combine(_iniDirPath, _name + _iniFormat);

            // Check File exist or not
            if (!System.IO.File.Exists(_filePath))
                throw new Exception("File doesn't exist in expected file path = " + _filePath);
        }

        #region Public Methods

        public void WriteIni(Dictionary<string, string> iniData)
        {
            try
            {
                foreach (var item in iniData)
                {
                    WritePrivateProfileString("config", item.Key, item.Value, _filePath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, string> ReadIni()
        {
            try
            {
                Dictionary<string, string> iniDic = new Dictionary<string, string>();
                string[] rawDatas = System.IO.File.ReadAllLines(_filePath, Encoding.UTF8);
                foreach (var rawData in rawDatas)
                {
                    int index = rawData.IndexOf('=');
                    if (index != -1)
                    {
                        char[] key = new char[index];
                        rawData.CopyTo(0, key, 0, index);
                        char[] val = new char[rawData.Length - index - 1];
                        rawData.CopyTo(index + 1, val, 0, rawData.Length - index - 1);
                        string Key = new string(key).Trim();
                        string Val = new string(val).Trim();

                        iniDic.Add(Key, Val);
                    }
                }
                return iniDic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
