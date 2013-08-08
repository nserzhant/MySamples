using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using System.Collections.ObjectModel;
using Downloader.Services;
using System.Threading;

namespace Downloader.ViewModels
{
    /// <summary>
    /// Responsible for management of downloading slots (calculate possible count,stops and starts all of them)
    /// </summary>
    public interface IDownloadSlotsViewModel
    {

        event Action DownloadCompleted;
        event Action DownloadFailed;
        event Action DownloadProgressChanged;

        IList<DownloadSlotViewModel> Items { get; }

        long ContentSize { get; }
        int CountSubDownloads { get; }
        long DownloadedCount { get; }
        double Speed { get; }

        void StartDownloading();
        void StopDownloading();
        bool TryInitialize();
    }

    public class DownloadSlotsViewModel : IDownloadSlotsViewModel
    {
        private object downloadSlotsLocker = new object();

        private ObservableCollection<DownloadSlotViewModel> items =
            new ObservableCollection<DownloadSlotViewModel>();

        public event Action DownloadCompleted = null;
        public event Action DownloadProgressChanged = null;
        public event Action DownloadFailed = null;


        private readonly IDownloadSlotsModel downloadSlotsModel = null;
        private readonly INetworkClientFactory downloadingClientFactory = null;
        private readonly IFileStreamClientFactory fileStreamClientFactory = null;

        private int downloading = 0;
        private int toDownloadLeft = 0;

        public DownloadSlotsViewModel(IDownloadSlotsModel downloadSlotsModel,
            INetworkClientFactory downloadingClientFactory,
            IFileStreamClientFactory fileStreamClientFactory)
        {
            if (downloadSlotsModel == null)
                throw new ArgumentNullException("downloadSlotsModel");
            if (downloadingClientFactory == null)
                throw new ArgumentNullException("downloadingClientFactory");
            if (fileStreamClientFactory == null)
                throw new ArgumentNullException("fileStreamClientFactory");

            this.downloadSlotsModel = downloadSlotsModel;
            this.downloadingClientFactory = downloadingClientFactory;
            this.fileStreamClientFactory = fileStreamClientFactory;
            this.fillDownloadSlots();
        }

        #region Properties
        public IList<DownloadSlotViewModel> Items
        {
            get
            {
                return items;
            }
        }

        public int CountSubDownloads
        {
            get { return Items.Count; }
        }

        public long ContentSize
        {
            get;
            private set;
        }

        public long DownloadedCount
        {
            get;
            private set;
        }

        public double Speed
        {
            get
            {
                return items.Sum(itm => itm.CurrentDownloadingSpeed);
            }
        }        
        #endregion

        #region Methods
        public void StartDownloading()
        {
            downloading = 0;

            var itemsToStart = (from item in items
                                where item.State != DownloadState.Downloaded
                                    && item.State != DownloadState.Downloading
                                select item).ToArray();

            downloading += itemsToStart.Length;
            toDownloadLeft = downloading;


            foreach (var item in itemsToStart)
            {
                item.Start();
            }
        }

        public void StopDownloading()
        {
            foreach (var item in items)
            {
                item.Stop();
            }
        }

        public bool TryInitialize()
        {
            this.StopDownloading();
            this.clear();
            if (this.tryCreateDownloadSlotsModels())
            {
                createDownloadSlotsViewModels();
                return true;
            }
            return false;
        }

        private void fillDownloadSlots()
        {
            this.resetSizeProperties();
            createDownloadSlotsViewModels();
        }


        private void clear()
        {
            foreach (var slot in items.ToArray())
            {
                slot.Remove();
            }
        }

        private void resetSizeProperties()
        {
            long downloadedCount = 0;
            long size = 0;
            foreach (var downloadedSlot in downloadSlotsModel.DownloadSlots)
            {
                downloadedCount += (downloadedSlot.CurrentPosition - downloadedSlot.StartPosition);
                if (downloadedSlot.FinishPosition == 0)
                {
                    size += downloadedSlot.CurrentPosition;
                }
                size = Math.Max(size, downloadedSlot.FinishPosition);
            }
            this.DownloadedCount = downloadedCount;
            this.ContentSize = size;
        }

        private bool tryCreateDownloadSlotsModels()
        {
            int avialableSlots = 0;
            var downloadingClient = downloadingClientFactory.CreateClient(downloadSlotsModel);
            avialableSlots = downloadingClient.GetAvialableDownloads();

            var contentProperties = downloadingClient.GetContentProperties();

            if (string.IsNullOrEmpty(contentProperties.FileName))
            {
                return false;
            }
            downloadSlotsModel.FileName = contentProperties.FileName;

            if (contentProperties.FileSize == 0 && avialableSlots > 0)
            {
                avialableSlots = createSingeThreadSlot(avialableSlots);
            }
            else
            {
                createMaxPossibleSlots(avialableSlots, contentProperties.FileSize);
            }
            if (avialableSlots == 0)
            {
                return false;
            }
            return true;
        }


        private void createMaxPossibleSlots(int avialableSlots, long contentSize)
        {
            for (int i = 1; i < avialableSlots + 1; i++)
            {
                long startPosition = contentSize - (((contentSize * i) / avialableSlots) + (contentSize * i) % avialableSlots);
                long finishPosition = contentSize - (((contentSize * (i - 1)) / avialableSlots) + (contentSize * (i - 1)) % avialableSlots);
                var downloadSlot = createDownloadSlot();
                downloadSlot.StartPosition = startPosition;
                downloadSlot.CurrentPosition = startPosition;
                downloadSlot.FinishPosition = finishPosition;
                downloadSlot.State = DownloadState.Stopped;
            }
        }

        private int createSingeThreadSlot(int avialableSlots)
        {
            avialableSlots = 1;
            var downloadSlot = createDownloadSlot();
            downloadSlot.StartPosition = 0;
            downloadSlot.CurrentPosition = 0;
            downloadSlot.FinishPosition = 0;
            downloadSlot.State = DownloadState.Stopped;
            return avialableSlots;
        }

        private void createDownloadSlotsViewModels()
        {
            foreach (var downloadSlot in downloadSlotsModel.DownloadSlots)
            {
                addDownloadSlot(downloadSlot);
            }
        }

        private void addDownloadSlot(DownloadSlot downloadSlot)
        {
            DownloadSlotViewModel downloadSlotViewModel = this.createDownloadSlotViewModel(downloadSlot);

            downloadSlotViewModel.OnRemove += () =>
                {
                    this.removeFromDownloadSlotModels(downloadSlot);
                    this.removeFromDownloadSlotViewModels(downloadSlotViewModel);
                };
            downloadSlotViewModel.OnDownloaded += downloadSlotViewModelOnDownloaded;
            downloadSlotViewModel.OnComplete += downloadSlotViewModelOnComplete;
            downloadSlotViewModel.OnError += downloadSlotViewModelOnError;
            downloadSlotViewModel.OnRestore += downloadSlotViewModelOnRestore;
        }


        private void removeFromDownloadSlotViewModels(DownloadSlotViewModel downloadSlotViewModel)
        {
            this.items.Remove(downloadSlotViewModel);
        }

        private void removeFromDownloadSlotModels(DownloadSlot downloadSlot)
        {
            this.downloadSlotsModel.DownloadSlots.Remove(downloadSlot);
        }

        private DownloadSlotViewModel createDownloadSlotViewModel(DownloadSlot downloadSlot)
        {
            DownloadSlotViewModel downloadSlotViewModel =
                new DownloadSlotViewModel(downloadSlot,
                    downloadingClientFactory.CreateClient(downloadSlotsModel),
                    fileStreamClientFactory.CreateFileStreamClient(downloadSlotsModel));
            this.items.Add(downloadSlotViewModel);
            return downloadSlotViewModel;
        }

        private DownloadSlot createDownloadSlot()
        {
            DownloadSlot downloadSlot = new DownloadSlot();
            this.downloadSlotsModel.DownloadSlots.Add(downloadSlot);
            return downloadSlot;
        }

        void downloadSlotViewModelOnRestore()
        {
            lock (downloadSlotsLocker)
            {
                downloading++;
            }
        }

        void downloadSlotViewModelOnError()
        {
            lock (downloadSlotsLocker)
            {
                if (--downloading == 0)
                {
                    onDownloadFailed();
                }
            }
        }

        void downloadSlotViewModelOnComplete()
        {
            lock (downloadSlotsLocker)
            {
                if (--toDownloadLeft == 0)
                {
                    onDownloadingCompleted();
                }
            }
        }

        void downloadSlotViewModelOnDownloaded(int count)
        {
            lock (downloadSlotsLocker)
            {
                resetSizeProperties();
                onDownloadProgressChanged();
            }
        }

        private void onDownloadingCompleted()
        {
            resetSizeProperties();
            if (DownloadCompleted != null)
            {
                DownloadCompleted();
            }
        }

        private void onDownloadProgressChanged()
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged();
            }
        }

        private void onDownloadFailed()
        {
            if (DownloadFailed != null)
            {
                DownloadFailed();
            }
        } 
        #endregion

    }
}
