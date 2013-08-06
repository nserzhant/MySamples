using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Downloader.Models;

namespace Downloader.Services
{
    /// <summary>
    /// Creates new instances of DownloadSlotsViewModel
    /// </summary>
    public interface IDownloadSlotsViewModelFactory
    {
        IDownloadSlotsViewModel CreateDownloadSlotsViewModel(Download download);
    }

    public class DownloadSlotsViewModelFactory : IDownloadSlotsViewModelFactory
    {
        private readonly INetworkClientFactory downloadingClientFactory = null;
        private readonly IFileStreamClientFactory fileStreamClientFactory = null;

        public DownloadSlotsViewModelFactory(INetworkClientFactory downloadingClientFactory,
            IFileStreamClientFactory fileStreamClientFactory)
        {
            if (downloadingClientFactory == null)
                throw new ArgumentNullException("downloadingClientFactory");
            if (fileStreamClientFactory == null)
                throw new ArgumentNullException("fileStreamClientFactory");

            this.downloadingClientFactory = downloadingClientFactory;
            this.fileStreamClientFactory = fileStreamClientFactory;
        }

        public IDownloadSlotsViewModel CreateDownloadSlotsViewModel(Download download)
        {
            IDownloadSlotsViewModel downloadSlotsViewModel =
                new DownloadSlotsViewModel(download, downloadingClientFactory, fileStreamClientFactory);
            return downloadSlotsViewModel;
        }
    }
}
