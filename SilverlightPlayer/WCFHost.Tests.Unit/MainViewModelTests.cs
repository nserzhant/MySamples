using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using WCFHost.ViewModels;
using Moq;
using WCFHost.Services;
using WebService;
using NUnit.Framework;

namespace WCFHost.Tests.Unit
{
    [Category("MainViewModel")]
    public class When_working_with_MainViewModel : SpecBase
    {
        protected MainViewModel MainViewModel = null;
        protected Mock<IWebServiceManager> WebServiceManager = null;

        protected override void Establish_context()
        {
            Mock<IServiceEndpointResolver> resolver = new Mock<IServiceEndpointResolver>();
            WebServiceManager = new Mock<IWebServiceManager>();
            string error = "";
            WebServiceManager.Setup(service => service.TryStart(It.IsAny<string>(), out error)).Returns(true);
            Mock<IErrorNotificationService> errorNotificationService = new Mock<IErrorNotificationService>();
            Mock<IStartUpBehavior> startUpBehavior = new Mock<IStartUpBehavior>();
            MainViewModel = new MainViewModel(resolver.Object, WebServiceManager.Object,
                errorNotificationService.Object, startUpBehavior.Object);
        }
    }

    public class And_executing_Start_command : When_working_with_MainViewModel
    {
        protected override void Because_of()
        {
            MainViewModel.StartCommand.Execute(null);
        }

        [Test]
        public void Then_web_service_manager_should_be_started()
        {
            string error;
            WebServiceManager.Verify(manager => manager.TryStart(It.IsAny<string>(), out error));
        }

        [Test]
        public void Then_web_service_manager_can_be_stopped()
        {
            MainViewModel.FinishCommand.CanExecute(null).ShouldEqual(true);
        }


    }

    public class And_executing_Stop_command : When_working_with_MainViewModel
    {
        protected override void Because_of()
        {
            MainViewModel.FinishCommand.Execute(null);
        }

        [Test]
        public void Then_web_service_manager_should_be_stopped()
        {
            WebServiceManager.Verify(manager => manager.ShootDown());
        }

        [Test]
        public void Then_web_service_manager_can_be_started()
        {
            MainViewModel.StartCommand.CanExecute(null).ShouldEqual(true);
        }
    }

    public class And_closing_application : When_working_with_MainViewModel
    {
        protected override void Because_of()
        {
            MainViewModel.Close();
        }

        [Test]
        public void Then_web_service_manager_should_be_stopped()
        {
            WebServiceManager.Verify(manager => manager.ShootDown());
        }
    }
}
