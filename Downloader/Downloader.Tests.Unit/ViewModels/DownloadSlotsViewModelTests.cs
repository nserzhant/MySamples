using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Downloader.Models;
using Downloader.Services;
using Moq;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace Downloader.Tests.Unit.ViewModels
{
    [Category("DownloadSlotsViewModel")]
    public class When_working_with_Download_SlotsViewModel : SpecBase
    {
        protected DownloadSlotsViewModel DownloadSlotsViewModel = null;
        protected Mock<IDownloadSlotsModel> DownloadSlotsModel = null;
        protected Mock<INetworkClientFactory> NetworkClientFactory = null;
        protected Mock<IFileStreamClientFactory> FileStreamClientFactory = null;
        protected Mock<IFileStreamClient> FileStreamClient = null;
        protected Mock<INetworkClient> NetworkClient = null;

        protected override void Establish_context()
        {
            base.Establish_context();

            DownloadSlotsModel = new Mock<IDownloadSlotsModel>();

            NetworkClientFactory = new Mock<INetworkClientFactory>();
            NetworkClient = new Mock<INetworkClient>();
            NetworkClientFactory.Setup(clientFactory => clientFactory.CreateClient(It.IsAny<IUriSource>()))
                .Returns(NetworkClient.Object);

            FileStreamClientFactory= new Mock<IFileStreamClientFactory>();
            FileStreamClient = new Mock<IFileStreamClient>();
            FileStreamClientFactory.Setup(clientFactory => clientFactory.CreateFileStreamClient(It.IsAny<IFilePathSource>()))
                .Returns(FileStreamClient.Object);                

            List<DownloadSlot> downloadSlots = this.createDownloadSlots();
            DownloadSlotsModel.SetupGet(slotsModel => slotsModel.DownloadSlots).Returns(downloadSlots);
            DownloadSlotsViewModel = new DownloadSlotsViewModel(DownloadSlotsModel.Object,
                NetworkClientFactory.Object, FileStreamClientFactory.Object);

        }

        protected virtual List<DownloadSlot> createDownloadSlots()
        {
            List<DownloadSlot> downloadSlots = new List<DownloadSlot>();
            downloadSlots.Add(new DownloadSlot()
            { 
                CurrentPosition = 0,
                StartPosition = 0,
                FinishPosition = 100,
                State = DownloadState.Stopped
            });
            downloadSlots.Add(new DownloadSlot()
            {
                CurrentPosition = 100,
                StartPosition = 50,
                FinishPosition = 1000,
                State = DownloadState.Downloaded
            });
            return downloadSlots;
        }
    }

    public class And_start_downloading_all_slots : When_working_with_Download_SlotsViewModel
    {
        protected override void Because_of()
        {
            DownloadSlotsViewModel.StartDownloading();
        }

        [Test]
        public void Then_all_except_downloaded_shoud_be_started()
        {
            int downloadingCount = (from item in DownloadSlotsViewModel.Items 
                                    where item.State ==  DownloadState.Downloading
                                        select item).Count();
            downloadingCount.ShouldEqual(1);
        }
    }

    public class And_stop_downloading_all_slots : When_working_with_Download_SlotsViewModel
    {
        protected override List<DownloadSlot> createDownloadSlots()
        {
            List<DownloadSlot> downloadSlots = new List<DownloadSlot>();
            downloadSlots.Add(new DownloadSlot()
            {
                CurrentPosition = 0,
                StartPosition = 0,
                FinishPosition = 100,
                State = DownloadState.Initializing
            });
            downloadSlots.Add(new DownloadSlot()
            {
                CurrentPosition = 100,
                StartPosition = 50,
                FinishPosition = 1000,
                State = DownloadState.Downloading
            });
            return downloadSlots;
        }

        protected override void Because_of()
        {
            DownloadSlotsViewModel.StopDownloading();
        }

        [Test]
        public void Then_no_downloading_slots_should_be_exists()
        {
            int downloadingCount = (from item in DownloadSlotsViewModel.Items
                                    where item.State == DownloadState.Downloading
                                    select item).Count();
            downloadingCount.ShouldEqual(0);
        }
    }

    public class And_initializing_download_slots : When_working_with_Download_SlotsViewModel
    {
        private readonly int canCreateCount = 100;

        private int countBefore = 0;

        protected override void Establish_context()
        {
            base.Establish_context();
            countBefore = DownloadSlotsViewModel.Items.Count;
            NetworkClient.Setup(client => client.GetAvialableDownloads()).Returns(100);
            NetworkClient.Setup(client => client.GetContentProperties()).Returns
                (new ContentProperties() 
                { 
                    FileName = "test",
                    FileSize = canCreateCount 
                });
        }

        protected override void Because_of()
        {
            DownloadSlotsViewModel.TryInitialize();
        }

        [Test]
        public void Then_download_slots_view_models_should_be_created()
        {
            int countAfter = DownloadSlotsViewModel.Items.Count;
            int created = countAfter - countBefore;
            created.ShouldEqual(canCreateCount - countBefore);
        }
    }
}
