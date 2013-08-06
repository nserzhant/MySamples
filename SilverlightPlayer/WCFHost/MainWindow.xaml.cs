using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WCFHost.ViewModels;

namespace WCFHost
{
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel = null;
        public MainWindow(MainViewModel mainViewModel)
        {
            if (mainViewModel == null)
                throw new ArgumentNullException("mainViewModel");

            InitializeComponent();
            this.DataContext = mainViewModel;
            this.mainViewModel = mainViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.mainViewModel.Close();
        }
    }
}
