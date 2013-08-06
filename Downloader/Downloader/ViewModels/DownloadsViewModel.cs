using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Downloader.Models;
using Downloader.Services;
using System.Threading.Tasks;

namespace Downloader.ViewModels
{

    /// <summary>
    /// Presents the whoole set of downloads, is responsible for creating new downloads and removing existed
    /// </summary>
    public interface IDownloadsViewModel
    {
        IEnumerable<IDownloadViewModel> Items { get; }
        void AddDownload(string Uri, string saveFolderPath);
    }

    public class DownloadsViewModel : IDownloadsViewModel
    {
        private ObservableCollection<IDownloadViewModel> items =
            new ObservableCollection<IDownloadViewModel>();

        private readonly IList<Download> downloads = null;
        private readonly IDownloadViewModelFactory downloadViewModelFactory = null;
        private readonly IDownloadSlotsViewModelAsyncFactory downloadSlotsViewModelAsyncFactory = null;

        public DownloadsViewModel(IList<Download> downloads,
            IDownloadViewModelFactory downloadViewModelFactory,
            IDownloadSlotsViewModelAsyncFactory downloadSlotsViewModelAsyncFactory)
        {
            if (downloads == null)
                throw new ArgumentNullException("downloads");
            if (downloadViewModelFactory == null)
                throw new ArgumentNullException("downloadViewModelFactory");
            if (downloadSlotsViewModelAsyncFactory == null)
                throw new ArgumentNullException("downloadSlotsViewModelAsyncFactory");

            this.downloads = downloads;
            this.downloadViewModelFactory = downloadViewModelFactory;
            this.downloadSlotsViewModelAsyncFactory = downloadSlotsViewModelAsyncFactory;
            this.downloadSlotsViewModelAsyncFactory.InitializationCompleted += 
                downloadSlotsViewModelAsyncFactoryInitializationCompleted;
            this.fillDownloads();
        }

        #region Properties
        public IEnumerable<IDownloadViewModel> Items
        {
            get { return items; }
        } 
        #endregion

        #region Methods
        void downloadSlotsViewModelAsyncFactoryInitializationCompleted(Download download, IDownloadSlotsViewModel initializedViewModel)
        {
            downloads.Add(download);
            var newDownloadViewModel = addDownloadViewModel(download, initializedViewModel);
            newDownloadViewModel.Start();
        }

        private void fillDownloads()
        {
            foreach (var download in downloads)
            {
                addDownload(download);
            }
        }

        public void AddDownload(string Uri, string saveFolderPath)
        {
            Download download = this.createDownloadModel(Uri, saveFolderPath);
            this.downloadSlotsViewModelAsyncFactory.TryInitializeDownloadSlotsViewModelAsync(download);
        }

        private void addDownload(Download download)
        {
            IDownloadSlotsViewModel downloadSlotsViewModel =
                this.downloadSlotsViewModelAsyncFactory.CreateDownloadSlotsViewModel(download);
            addDownloadViewModel(download, downloadSlotsViewModel);
        }

        private IDownloadViewModel addDownloadViewModel(Download download, IDownloadSlotsViewModel downloadSlotsViewModel)
        {
            IDownloadViewModel downloadViewModel = this.createDownloadViewModel(download, downloadSlotsViewModel);

            downloadViewModel.OnRemove += () =>
            {
                this.removeFromDownloadModels(download);
                this.removeFromDownloadViewModels(downloadViewModel);
            };

            return downloadViewModel;
        }

        private void removeFromDownloadViewModels(IDownloadViewModel downloadViewModel)
        {
            items.Remove(downloadViewModel);
        }

        private void removeFromDownloadModels(Download download)
        {
            downloads.Remove(download);
        }

        private IDownloadViewModel createDownloadViewModel(Download download,
            IDownloadSlotsViewModel downloadSlotsViewModel)
        {
            IDownloadViewModel downloadViewModel = downloadViewModelFactory.CreateDownloadViewModel(download,
                downloadSlotsViewModel);
            items.Add(downloadViewModel);
            return downloadViewModel;
        }

        private Download createDownloadModel(string Uri, string saveFolderPath)
        {
            Download download = new Download(Uri);
            download.State = DownloadState.Initializing;
            download.SaveFolderPath = saveFolderPath;
            return download;
        } 
        #endregion
    }
}
