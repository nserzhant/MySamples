using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using Downloader.MVVM;
using Downloader.Services;

namespace Downloader.ViewModels
{
    /// <summary>
    /// Responsible for management of downloading slots (calculate possible count,stops and starts all of them)
    /// </summary>
    public interface IDownloadSlotViewModel
    {
        event Action OnComplete;
        event Action<int> OnDownloaded;
        event Action OnError;
        event Action OnRemove;
        event Action OnRestore;

        double CurrentDownloadingSpeed { get; }
        long CurrentPosition { get; set; }
        long FinishPosition { get; set; }
        long StartPosition { get; set; }
        DownloadState State { get; set; }

        void Stop();
        void Remove();
        void Start();
    }

    public class DownloadSlotViewModel : ViewModelBase, IDownloadSlotViewModel
    {
        public event Action OnRemove = null;
        public event Action<int> OnDownloaded = null;
        public event Action OnComplete = null;
        public event Action OnError = null;
        public event Action OnRestore = null;

        private readonly INetworkClient downloadingClient = null;
        private readonly IFileStreamClient fileStreamClient = null;

        private readonly DownloadSlot downloadSlot = null;

        public DownloadSlotViewModel(DownloadSlot downloadSlot, INetworkClient downloadingClient,
            IFileStreamClient fileStreamClient)
        {
            if (downloadSlot == null)
                throw new ArgumentNullException("downloadSlot");
            if (downloadingClient == null)
                throw new ArgumentNullException("downloadingClient");
            if (fileStreamClient == null)
                throw new ArgumentNullException("fileStreamClient");

            this.downloadSlot = downloadSlot;
            this.downloadingClient = downloadingClient;
            this.fileStreamClient = fileStreamClient;

            this.downloadingClient.OnBlockDownloaded += downloadingClientOnBlockDownloaded;
            this.downloadingClient.OnError += downloadingClientOnError;
            this.downloadingClient.OnRestore += downloadingClientOnRestore;
            this.downloadingClient.OnComplete += downloadingClientOnComplete;
        }

        #region Properties

        public DownloadState State
        {
            get { return downloadSlot.State; }
            set
            {
                if (downloadSlot.State != value)
                {
                    downloadSlot.State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public long StartPosition
        {
            get { return downloadSlot.StartPosition; }
            set
            {
                if (downloadSlot.StartPosition != value)
                {
                    downloadSlot.StartPosition = value;
                    NotifyPropertyChanged("StartPosition");
                }
            }
        }

        public long FinishPosition
        {
            get { return downloadSlot.FinishPosition; }
            set
            {
                if (downloadSlot.FinishPosition != value)
                {
                    downloadSlot.FinishPosition = value;
                    NotifyPropertyChanged("FinishPosition");
                }
            }
        }

        public long CurrentPosition
        {
            get { return downloadSlot.CurrentPosition; }
            set
            {
                if (downloadSlot.CurrentPosition != value)
                {
                    downloadSlot.CurrentPosition = value;
                    NotifyPropertyChanged("CurrentPosition");
                }
            }
        }

        public double CurrentDownloadingSpeed
        {
            get
            {
                return downloadingClient.CurrentSpeed;
            }
        } 
        #endregion

        #region Methods

        void downloadingClientOnComplete()
        {
            Stop();
            State = DownloadState.Downloaded;
            onComplete();
        }

        void downloadingClientOnRestore()
        {
            State = DownloadState.Downloading;
            OnRestore();
        }

        void downloadingClientOnError()
        {
            State = DownloadState.Terminated;
            OnError();
        }

        public void Start()
        {
            fileStreamClient.Open(CurrentPosition);
            downloadingClient.StartDownloading(CurrentPosition, FinishPosition);
            State = DownloadState.Downloading;
        }

        public void Stop()
        {
            if (State != DownloadState.Downloaded && State != DownloadState.Stopped)
            {
                State = DownloadState.Stopped;
                downloadingClient.StopDownloading();
                fileStreamClient.Close();
            }
        }

        public void Remove()
        {
            if (OnRemove != null)
            {
                OnRemove();
                OnRemove = null;
            }
        }

        void downloadingClientOnBlockDownloaded(byte[] buffer, int count)
        {
            fileStreamClient.Write(buffer, count);
            CurrentPosition += count;
            onDownloaded(count);
        }

        private void onDownloaded(int count)
        {
            if (OnDownloaded != null)
            {
                OnDownloaded(count);
            }
        }

        private void onComplete()
        {
            if (OnComplete != null)
            {
                OnComplete();
                OnComplete = null;
            }
        }

        private void onError()
        {
            if (OnError != null)
            {
                OnError();
            }
        }

        private void onRestore()
        {
            if (OnRestore != null)
            {
                OnRestore();
            }
        }
        
        #endregion
    }
}
