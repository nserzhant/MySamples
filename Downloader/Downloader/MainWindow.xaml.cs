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
using Downloader.ViewModels;
using Downloader.Services;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IStateSerializer stateSerializer = null;
        ILogger logger = null;

        public MainWindow(MainViewModel mainViewModel, IStateSerializer stateSerializer,ILogger logger)
        {
            if (mainViewModel == null)
                throw new ArgumentNullException("mainViewModel");
            if (stateSerializer == null)
                throw new ArgumentNullException("stateSerializer");
            if (logger == null)
                throw new ArgumentNullException("logger");

            InitializeComponent();
            this.DataContext = mainViewModel;
            this.stateSerializer = stateSerializer;
            this.logger = logger;

            this.Closing += MainWindow_Closing;
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            NotificationService.Alert("Unhandled exception throwed when processing asyncronous operation",
                "exception");
            logger.LogException(e.Exception);
        }

        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            NotificationService.Alert("Unhandled exception detected in UI thread",
                "exception");
            logger.LogException(e.Exception);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.stateSerializer.Save();
        }
    }
}
