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
using DevTool.File;
using WpfWebScrapper.ViewModel;
using WpfWebScrapper.Model;

namespace WpfWebScrapper.View
{
    /// <summary>
    /// Main.xaml 的互動邏輯
    /// </summary>
    public partial class V_Main : Page
    {
        private VM_VMList vmList;

        public V_Main()
        {
            InitializeComponent();

            var viewModel = (VM_Main)DataContext;
            var model = new Main(viewModel);
            vmList = model.GetVMList();
        }

        private void Button_Click_Engineering(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new V_Engineering(vmList.Engineering));
        }

        private void Button_Click_QuitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
