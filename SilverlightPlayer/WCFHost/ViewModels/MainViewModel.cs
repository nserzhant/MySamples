using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCFHost.MVVM;
using WCFHost.Services;
using WebService;

namespace WCFHost.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly int DEFAULT_PORT = 4480;
        private readonly IServiceEndpointResolver serviceEndpointResolver = null;
        private readonly IWebServiceManager webServiceManager = null;
        private readonly IErrorNotificationService errorNotificationService = null;
        private readonly IStartUpBehavior startUpBehavior = null;

        private List<string> endpoints = null;

        private string currentEndpoint = null;
        private int port;
        private bool canStart = true;

        public MainViewModel(IServiceEndpointResolver serviceEndpointResolver,
            IWebServiceManager webServiceManager,
            IErrorNotificationService errorNotificationService,
            IStartUpBehavior startUpBehavior)
        {
            if (serviceEndpointResolver == null)
                throw new ArgumentNullException("serviceEndpointResolver");
            if (webServiceManager == null)
                throw new ArgumentNullException("webServiceManager");
            if (errorNotificationService == null)
                throw new ArgumentNullException("errorNotificationService");
            if (startUpBehavior == null)
                throw new ArgumentNullException("startUpBehavior");

            this.serviceEndpointResolver = serviceEndpointResolver;
            this.webServiceManager = webServiceManager;
            this.errorNotificationService = errorNotificationService;
            this.startUpBehavior = startUpBehavior;
            this.fillEndpoints();
            port = DEFAULT_PORT;
        }

        private void fillEndpoints()
        {
            endpoints = new List<string>();
            endpoints.AddRange(serviceEndpointResolver.GetAvialableEndpoints());
        }

        public List<string> Endpoints
        {
            get { return endpoints; }
        }

        public string CurrentEndpoint
        {
            get { return currentEndpoint; }
            set
            {
                if (currentEndpoint != value)
                {
                    currentEndpoint = value;
                    NotifyPropertyChanged("CurrentEndpoint");
                }
            }
        }

        public int Port
        {
            get { return port; }
            set
            {
                if (port != value)
                {
                    port = value;
                    NotifyPropertyChanged("Port");
                }
            }
        }

        public RelayCommand StartCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                    {
                        this.start();
                    }, obj => canStart);
            }
        }

        public RelayCommand FinishCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                    {
                        this.finish();
                    }, obj => !canStart);
            }
        }

        public void Close()
        {
            webServiceManager.ShootDown();
        }

        private void finish()
        {
            webServiceManager.ShootDown();
            canStart = true;
        }

        private void start()
        {
            string uri = this.getCurrentEndpointUri();
            string errorMessage;
            if (this.webServiceManager.TryStart(uri, out errorMessage))
            {
                canStart = false;
                startUpBehavior.OnStart(uri);
            }
            else
            {
                string displayError = 
                    string.Format("Unable to startService {0}{1}", Environment.NewLine, errorMessage);
                errorNotificationService.Notify(displayError);
            }
        }

        private string getCurrentEndpointUri()
        {
            string ipAddress = string.IsNullOrEmpty(currentEndpoint) ?
                "localhost" :
                currentEndpoint;
            string address = string.Format("http://{0}:{1}/", ipAddress, port);
            return address;
        }
    }
}
