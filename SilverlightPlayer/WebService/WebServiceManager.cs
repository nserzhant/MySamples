using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using Castle.Facilities.WcfIntegration;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace WebService
{
    public class WebServiceManager : IWebServiceManager
    {
        private readonly string contentPath = @"content\videos";
        private readonly string errorPagePath = @"content\error.html";
        private ServiceHost serviceHost = null;

        public WebServiceManager(string contentPath, string errorPagePath)
        {
            this.contentPath = contentPath;
            this.errorPagePath = errorPagePath;
        }

        public bool TryStart(string endpointAddress, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                this.ShootDown();
                serviceHost = createServiceHost();
                WebHttpBinding restBinding = new WebHttpBinding()
                {
                    TransferMode = TransferMode.Buffered,//TransferMode.StreamedResponse,
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue
                };
                ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(IWebService)
                    , restBinding, endpointAddress);
                endpoint.Behaviors.Add(new WebHttpBehavior());
                serviceHost.Open();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            return true;
        }

        private ServiceHost createServiceHost()
        {
            WindsorContainer container = new WindsorContainer();
            container.AddFacility<WcfFacility>();
            container.Register(
                Component.For<WebService>().
                DependsOn(Dependency.OnValue("contentSubPath", contentPath))
                .DependsOn(Dependency.OnValue("errorPageTemplatePath", errorPagePath))
                );
            DefaultServiceHostFactory castleWindsorFactory =
                new DefaultServiceHostFactory(container.Kernel);
            var windsorServiceHost = (ServiceHost)castleWindsorFactory.
                CreateServiceHost(typeof(WebService).AssemblyQualifiedName, new Uri[] { });
            return windsorServiceHost;
        }

        public void ShootDown()
        {
            if (serviceHost != null && serviceHost.State == CommunicationState.Opened)
            {
                serviceHost.Close();
            }
        }
    }
}
