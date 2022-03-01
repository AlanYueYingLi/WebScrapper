using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTool.File;
using WpfWebScrapper.ViewModel;

namespace WpfWebScrapper.Model
{
    public class Main
    {
        private readonly VM_Main viewModel;
        private VM_VMList vmList;
        private IniFileHandler _iniFileHandler;
        private Dictionary<string, string> _iniDic;
        private Engineering _engineering;

        public Main(VM_Main ViewModel)
        {
            // Assign the viewModel
            viewModel = ViewModel;

            // Create IniFileHandler
            _iniFileHandler = new IniFileHandler(nameof(Main));
            // Get IniDictionary
            _iniDic = _iniFileHandler.ReadIni();
            this.UpdateVMWithIniSetting();

            // Do initialization
            this.InitializeViewModels();
            this.InitializeModels();
        }

        public VM_VMList GetVMList()
        {
            if (vmList != null)
                return vmList;
            else
                throw new Exception("LVMist is null!");
        }

        private void UpdateVMWithIniSetting()
        {
            viewModel.AppName = _iniDic[nameof(viewModel.AppName)];
            viewModel.AppVersion = _iniDic[nameof(viewModel.AppVersion)];
        }

        private void InitializeViewModels()
        {
            vmList = new VM_VMList();
            vmList.Main = viewModel;
            vmList.Engineering = new VM_Engineering();
        }

        private void InitializeModels()
        {
            _engineering = new Engineering(vmList.Engineering);
        }
    }
}
