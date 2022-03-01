using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTool.MVVM;

namespace WpfWebScrapper.ViewModel
{
    public class VM_VMList : ViewModelBase
    {
        public VM_Main Main { get; set; }
        public VM_Engineering Engineering { get; set; }
    }
}
