using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using Downloader.ViewModels;

namespace Downloader.Services
{
    /// <summary>
    /// Creates new instances of DownloadViewModel class
    /// </summary>
    public interface IDownloadViewModelFactory
    {
        IDownloadViewModel CreateDownloadViewModel(Download download,IDownloadSlotsViewModel downloadSlotsViewModel);
    }

    public class DownloadViewModelFactory : IDownloadViewModelFactory
    {
        private readonly IFileSystemManager fileSystemManager = null;

        public DownloadViewModelFactory(IFileSystemManager fileSystemManager)
        {
            if (fileSystemManager == null)
                throw new ArgumentNullException("fileSystemManager");

            this.fileSystemManager = fileSystemManager;
        }


        public IDownloadViewModel CreateDownloadViewModel(Download download,
            IDownloadSlotsViewModel downloadSlotsViewModel)
        {
            DownloadViewModel downloadViewModel =
                new DownloadViewModel(download, downloadSlotsViewModel, fileSystemManager);
            return downloadViewModel;
        }
    }
}
