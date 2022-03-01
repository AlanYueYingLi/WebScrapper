using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevTool.File;
using WpfWebScrapper.Service;
using WpfWebScrapper.ViewModel;

namespace WpfWebScrapper.Model
{
    public class Engineering
    {
        private readonly VM_Engineering viewModel;
        private IniFileHandler _iniFileHandler;
        private Dictionary<string, string> _iniDic;
        private System.Timers.Timer _exeTimer;
        private const double ExeTimerInterval = 100;

        public Engineering(VM_Engineering ViewModel)
        {
            // Assign the viewModel
            viewModel = ViewModel;

            // Create IniFileHandler
            _iniFileHandler = new IniFileHandler(nameof(Engineering));
            // Get IniDictionary
            _iniDic = _iniFileHandler.ReadIni();
            this.UpdateVMWithIniSetting();

            // Create Execution thread timer
            this.CreateExeThread();
        }

        ~Engineering()
        {
            this.DestructExeThread();
        }

        private void doWebScrape()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var ticker = viewModel.Ticker;
            // Get html
            var response = WebScrape.CallUrl(viewModel.ETFDataBase + ticker).Result;

            // Do data parsing

            // Export data to File or DB

            stopWatch.Stop();
            viewModel.Response = response;
            viewModel.EllapsedTime = stopWatch.ElapsedMilliseconds;
        }

        private void UpdateVMWithIniSetting()
        {
            viewModel.ETFDataBase = _iniDic[nameof(viewModel.ETFDataBase)];
        }

        private void CreateExeThread()
        {
            _exeTimer = new System.Timers.Timer(ExeTimerInterval);
            _exeTimer.AutoReset = true;
            _exeTimer.Elapsed += _exeTimer_Elapsed;
            _exeTimer.Start();
        }

        private void DestructExeThread()
        {
            _exeTimer.Elapsed -= _exeTimer_Elapsed;
            _exeTimer.Stop();
        }

        private void _exeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (viewModel.CmdList.Count != 0)
            {
                // Stop the timer during execution
                _exeTimer.Stop();

                // Dequeue CmdList
                var cmd = viewModel.CmdList.Dequeue();
                switch (cmd)
                {
                    case VM_Engineering.EngineeringCmdType.Test:
                        doWebScrape();
                        break;
                    default:
                        throw new InvalidOperationException("No corresponding operation implemented!");
                }

                // Restart the timer after execution completed
                _exeTimer.Start();
            }
        }
    }
}
