using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Downloader.Models;
using System.Threading.Tasks;

namespace Downloader.Services
{
    /// <summary>
    /// Creates and asyncronously initializes (and if initialization has been success, notify the client to proceed downloading new file) 
    /// DownloadSlots viewModels
    /// </summary>
    public interface IDownloadSlotsViewModelAsyncFactory
    {
        event Action<Download, IDownloadSlotsViewModel> InitializationCompleted;
        void TryInitializeDownloadSlotsViewModelAsync(Download download);
        IDownloadSlotsViewModel CreateDownloadSlotsViewModel(Download download);
    }

    public class DownloadSlotsViewModelAsyncFactory : IDownloadSlotsViewModelAsyncFactory
    {
        public event Action<Download, IDownloadSlotsViewModel> InitializationCompleted = null;

        private readonly ILogger logger = null;
        private readonly IDownloadSlotsViewModelFactory downloadSlotsViewModelFactory = null;

        public DownloadSlotsViewModelAsyncFactory(IDownloadSlotsViewModelFactory downloadSlotsViewModelFactory,
            ILogger logger)
        {
            if (downloadSlotsViewModelFactory == null)
                throw new ArgumentNullException("downloadSlotsViewModelFactory");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.downloadSlotsViewModelFactory = downloadSlotsViewModelFactory;
            this.logger = logger;
        }


        public void TryInitializeDownloadSlotsViewModelAsync(Download download)
        {
            IDownloadSlotsViewModel downloadSlotsViewModel = CreateDownloadSlotsViewModel(download);

            Task<bool> initializeDownloadingSlotsTask = new Task<bool>(downloadSlotsViewModel.TryInitialize);

            initializeDownloadingSlotsTask.ContinueWith((faultedTask) =>
                {
                    logger.LogException(faultedTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);

            initializeDownloadingSlotsTask.ContinueWith((completedTask) =>
            {
                if (completedTask.Status != TaskStatus.Faulted && completedTask.Result)
                {
                    raiseInitializationCompleted(download, downloadSlotsViewModel);
                }
                else
                {
                    NotificationService.Alert(Properties.Resources.DownloadStartFailedMessage,
                        Properties.Resources.DownloadingFailedHeader);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            initializeDownloadingSlotsTask.Start();
        }

        private void raiseInitializationCompleted(Download download, IDownloadSlotsViewModel downloadSlotsViewModel)
        {
            if (InitializationCompleted != null)
            {
                InitializationCompleted(download, downloadSlotsViewModel);
            }
        }

        public IDownloadSlotsViewModel CreateDownloadSlotsViewModel(Download download)
        {
            return downloadSlotsViewModelFactory.CreateDownloadSlotsViewModel(download);
        }
    }
}
