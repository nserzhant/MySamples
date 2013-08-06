using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using Downloader.MVVM;
using Downloader.Services;
using System.IO;

namespace Downloader.ViewModels
{
    /// <summary>
    /// Drives the single download
    /// </summary>
    public interface IDownloadViewModel
    {
        event Action OnRemove;

        int CountSubDownloads { get; }
        string DownloadProgress { get; }
        string DownloadProgressText { get; }
        string Speed { get; }
        DownloadState State { get; set; }
        string URI { get; set; }
        IDownloadSlotsViewModel DownloadSlotsViewModel { get; }

        RelayCommand ChangeUriCommand { get; }
        RelayCommand ContinueCommand { get; }
        RelayCommand OpenFileCommand { get; }
        RelayCommand OpenFolderCommand { get; }
        RelayCommand PauseCommand { get; }
        RelayCommand RemoveCommand { get; }
        RelayCommand RemoveFileCommand { get; }

        void Start();
    }

    public class DownloadViewModel : ViewModelBase, IDownloadViewModel
    {
        public event Action OnRemove = null;

        private readonly IDownloadSlotsViewModel downloadSlotsViewModel = null;
        private readonly IDownloadModel downloadModel = null;
        private readonly IFileSystemManager fileSystemManager = null;

        public DownloadViewModel(IDownloadModel downloadModel,
            IDownloadSlotsViewModel downloadSlotsViewModel,
            IFileSystemManager fileSystemManager)
        {
            if (downloadModel == null)
                throw new ArgumentNullException("download");
            if (downloadSlotsViewModel == null)
                throw new ArgumentNullException("downloadSlotsViewModel");
            if (fileSystemManager == null)
                throw new ArgumentNullException("fileSystemManager");

            this.downloadModel = downloadModel;
            this.downloadSlotsViewModel = downloadSlotsViewModel;
            this.fileSystemManager = fileSystemManager;

            this.downloadSlotsViewModel.DownloadCompleted += downloadSlotsViewModel_DownloadCompleted;
            this.downloadSlotsViewModel.DownloadProgressChanged += downloadSlotsViewModel_DownloadProgressChanged;
            this.downloadSlotsViewModel.DownloadFailed += downloadSlotsViewModel_DownloadFailed;
        }


        #region Properties

        public IDownloadSlotsViewModel DownloadSlotsViewModel
        {
            get
            {
                return downloadSlotsViewModel;
            }
        }


        public DownloadState State
        {
            get { return downloadModel.State; }
            set
            {
                if (downloadModel.State != value)
                {
                    downloadModel.State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public string DownloadProgress
        {
            get
            {
                double size = downloadSlotsViewModel.ContentSize * 1.0 / 1024;
                string downloadedKbText = String.Format("{0:0,0,0}Kb", size);
                return downloadedKbText;
            }
        }

        public string DownloadProgressText
        {
            get
            {
                double percentage = (downloadSlotsViewModel.ContentSize == 0) ?
                    0.0 :
                    100.0 * downloadSlotsViewModel.DownloadedCount / downloadSlotsViewModel.ContentSize;
                string progressText = String.Format("{0:00.00}%", percentage);
                return progressText;
            }
        }

        public string Speed
        {
            get
            {
                double speed = downloadSlotsViewModel.Speed * 1.0 / 1024;
                string speedInKb = String.Format("{0:0,0} Kb/Sec.", speed);
                return speedInKb;
            }
        }

        /// <summary>
        /// Download uri
        /// </summary>
        public string URI
        {
            get { return downloadModel.Uri; }
            set
            {
                if (downloadModel.Uri != value)
                {
                    downloadModel.Uri = value;
                    NotifyPropertyChanged("URI");
                }
            }
        }

        private string saveFilePath
        {
            get
            {
                return Path.Combine(downloadModel.SaveFolderPath, downloadModel.FileName);
            }
        }

        public int CountSubDownloads
        {
            get { return downloadSlotsViewModel.Items.Count; }
        }

        #endregion

        #region Commands

        public RelayCommand PauseCommand
        {
            get
            {
                return new RelayCommand(stop, obj =>
                    State == DownloadState.Downloading);
            }
        }

        public RelayCommand ContinueCommand
        {
            get
            {
                return new RelayCommand(download, obj =>
                    State == DownloadState.Stopped);
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return new RelayCommand(remove, null);
            }
        }

        public RelayCommand ChangeUriCommand
        {
            get
            {
                return new RelayCommand(changeUri, obj =>
                    State != DownloadState.Downloaded);
            }
        }

        public RelayCommand OpenFolderCommand
        {
            get
            {
                return new RelayCommand(obj =>
                    fileSystemManager.OpenFolder(saveFilePath),
                    obj => State == DownloadState.Downloaded);
            }
        }

        public RelayCommand OpenFileCommand
        {
            get
            {
                return new RelayCommand(obj =>
                fileSystemManager.OpenFile(saveFilePath),
                obj => State == DownloadState.Downloaded);
            }
        }

        public RelayCommand RemoveFileCommand
        {
            get
            {
                return new RelayCommand(removeFile,
                obj => State == DownloadState.Downloaded);
            }
        }
        
        #endregion

        #region Methods

        public void Start()
        {
            if (State == DownloadState.Initializing)
            {
                download();
            }
        }

        void downloadSlotsViewModel_DownloadFailed()
        {
            NotificationService.Alert(Properties.Resources.DownloadingFailedMessage,
                Properties.Resources.DownloadingFailedHeader);
            stop();
        }

        void downloadSlotsViewModel_DownloadProgressChanged()
        {
            NotifyPropertyChanged("DownloadProgress");
            NotifyPropertyChanged("DownloadProgressText");
            NotifyPropertyChanged("Speed");
        }

        void downloadSlotsViewModel_DownloadCompleted()
        {
            State = DownloadState.Downloaded;
            downloadModel.FileName = fileSystemManager.RenameTempFile(saveFilePath);
            NotifyPropertyChanged("ChangeUriCommand");
            NotificationService.Inform(Properties.Resources.DownloadCompletedMessage
                , Properties.Resources.DownloadCompletedHeader);
        }

        private void removeFile(object item = null)
        {
            fileSystemManager.RevmoveFile(saveFilePath);
        }

        private void stop(object item = null)
        {
            State = DownloadState.Stopped;
            downloadSlotsViewModel.StopDownloading();
        }

        private void download(object item = null)
        {
            State = DownloadState.Downloading;
            downloadSlotsViewModel.StartDownloading();
        }

        private void remove(object item = null)
        {
            stop();
            removeFile();
            if (OnRemove != null)
            {
                OnRemove();
                OnRemove = null;
            }
        }

        private void changeUri(object item)
        {
            stop(null);
            download(null);
        }
        
        #endregion
    }
}
