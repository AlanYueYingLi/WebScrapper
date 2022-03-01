using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTool.MVVM;

namespace WpfWebScrapper.ViewModel
{
    public class VM_Engineering : ViewModelBase
    {
        public string ETFDataBase { get; set; }
        public string Ticker { get; set; }
        public string Response { get; set; }
        public double EllapsedTime { get; set; }

        public Queue<EngineeringCmdType> CmdList { get; set; }

        public enum EngineeringCmdType
        {
            Test,
        }

        public VM_Engineering()
        {
            CmdList = new Queue<EngineeringCmdType>();
        }
    }
}
