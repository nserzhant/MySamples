using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Downloader.Models;
using Downloader.ViewModels;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Downloader.Services;
using System.Windows.Threading;
using System.Threading.Tasks;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            WindsorContainer container = new WindsorContainer();

            container.Register(Component.For<ILogger>().ImplementedBy<Logger>());
            container.Register(Component.For<IStateSerializer>().ImplementedBy<BinaryStateSerializer>()
                .LifestyleSingleton());
            container.Register(Component.For<IFileSystemManager>().ImplementedBy<FileSystemManager>());
            container.Register(Component.For<ISettings,ISaveFileSettings,INetworkSettings>().ImplementedBy<AppConfigSettings>()
                .LifestyleSingleton());
            container.Register(Component.For<ISpeedMeasurerFactory>().ImplementedBy<SpeedMeasurerFactory>());
            container.Register(Component.For<INetworkClientFactory>().ImplementedBy<NetworkClientFactory>());
            container.Register(Component.For<IFileStreamClientFactory>().ImplementedBy<FileStreamClientFactory>());
            container.Register(Component.For<IDownloadViewModelFactory>().ImplementedBy<DownloadViewModelFactory>());
            container.Register(Component.For<IDownloadSlotsViewModelFactory>().ImplementedBy<DownloadSlotsViewModelFactory>());
            container.Register(Component.For<IDownloadSlotsViewModelAsyncFactory>().
                ImplementedBy<DownloadSlotsViewModelAsyncFactory>());
            
            container.Register(Component.For<IMainModel>().UsingFactoryMethod(kernel =>
                kernel.Resolve<IStateSerializer>().CurrentState));
            container.Register(Component.For<IList<Download>>().UsingFactoryMethod(kernel =>
                kernel.Resolve<IStateSerializer>().CurrentState.Downloads));

            container.Register(Component.For <ISettingsViewModel>()
                .ImplementedBy<SettingsViewModel>().LifestyleSingleton());
            container.Register(Component.For<IDownloadsViewModel>()
                .ImplementedBy<DownloadsViewModel>().LifestyleSingleton());
            container.Register(Component.For<MainViewModel>().LifestyleSingleton());

            container.Register(Component.For<MainWindow>());
            MainWindow mainWindow = container.Resolve<MainWindow>();            
            mainWindow.Show();
        }
    }
}
