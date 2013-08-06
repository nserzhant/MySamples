using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using WCFHost.ViewModels;
using WCFHost.Services;
using WebService;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace WCFHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string contentPath = @"content\videos";
            string errorPagePath = @"content\error.html";

            WindsorContainer container = new WindsorContainer();

            container.Register(Component.For<IWebServiceManager>().ImplementedBy<WebServiceManager>().
                DependsOn(Dependency.OnValue("contentPath", contentPath)).
                DependsOn(Dependency.OnValue("errorPagePath", errorPagePath)).
                LifestyleSingleton());
            container.Register(Component.For<IServiceEndpointResolver>().ImplementedBy<ServiceEndpointResolver>());
            container.Register(Component.For<IErrorNotificationService>().ImplementedBy<ErrorNotificationService>());
            container.Register(Component.For<IStartUpBehavior>().ImplementedBy<StartUpBehavior>());
            container.Register(Component.For<MainViewModel>());
            container.Register(Component.For<MainWindow>());

            MainWindow window = container.Resolve<MainWindow>();
            window.Show();
        }
    }
}
