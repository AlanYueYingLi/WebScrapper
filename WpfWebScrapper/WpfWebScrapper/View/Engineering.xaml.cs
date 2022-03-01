using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfWebScrapper.ViewModel;
using static WpfWebScrapper.ViewModel.VM_Engineering;

namespace WpfWebScrapper.View
{
    /// <summary>
    /// Engineering.xaml 的互動邏輯
    /// </summary>
    public partial class V_Engineering : Page
    {
        private readonly VM_Engineering viewModel;

        public V_Engineering(VM_Engineering ViewModel)
        {
            InitializeComponent();

            viewModel = ViewModel;
            DataContext = viewModel;
        }

        private void Button_Click_Test(object sender, RoutedEventArgs e)
        {
            viewModel.CmdList.Enqueue(EngineeringCmdType.Test);
        }
    }
}
