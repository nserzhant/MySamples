using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Moq;
using Downloader.Models;
using Downloader.Services;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace Downloader.Tests.Unit.ViewModels
{

    [Category("DownloadSlotViewModel")]
    public class When_working_with_download_slot : SpecBase
    {
        protected DownloadSlotViewModel DownloadSlotViewModel = null;
        protected DownloadSlot DownloadSlot = null;
        protected Mock<INetworkClient> NetworkClient = null;
        protected Mock<IFileStreamClient> FileStreamClient = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            DownloadSlot = InitDownloadSlot();
            NetworkClient = new Mock<INetworkClient>();
            FileStreamClient = new Mock<IFileStreamClient>();
            DownloadSlotViewModel = new DownloadSlotViewModel(DownloadSlot, NetworkClient.Object, FileStreamClient.Object);
        }

        protected virtual DownloadSlot InitDownloadSlot()
        {
            return new DownloadSlot();
        }
    }

    public class And_start_downloading : When_working_with_download_slot
    {
        private readonly long startPosition = 0;
        private readonly long currentPosition = 50;
        private readonly long finishPosition = 100;

        protected override DownloadSlot InitDownloadSlot()
        {
            DownloadSlot downloadSlot = new DownloadSlot();
            downloadSlot.StartPosition = startPosition;
            downloadSlot.CurrentPosition = currentPosition;
            downloadSlot.FinishPosition = finishPosition;
            downloadSlot.State = DownloadState.Initializing;
            return downloadSlot;
        }

        protected override void Because_of()
        {
            DownloadSlotViewModel.Start();
        }

        [Test]
        public void Then_file_for_writing_should_be_opened()
        {
            FileStreamClient.Verify(fileStreamClient =>
                fileStreamClient.Open(currentPosition));
        }

        [Test]
        public void Then_Network_client_have_to_start_downloading()
        {
            NetworkClient.Verify(client =>
                client.StartDownloading(currentPosition, finishPosition));
        }

        [Test]
        public void Then_state_should_be_downloading()
        {
            DownloadSlotViewModel.State.ShouldEqual(DownloadState.Downloading);
        }
    }

    public class And_stop_downloading : When_working_with_download_slot
    {

        protected override DownloadSlot InitDownloadSlot()
        {
            DownloadSlot downloadSlot = new DownloadSlot();
            downloadSlot.State = DownloadState.Downloading;
            return downloadSlot;
        }

        protected override void Because_of()
        {
            DownloadSlotViewModel.Stop();
        }

        [Test]
        public void Then_file_should_be_closed()
        {
            FileStreamClient.Verify(fileStreamClient => fileStreamClient.Close());
        }

        [Test]
        public void Then_downloading_process_should_be_stopped()
        {
            NetworkClient.Verify(client => client.StopDownloading());
        }

        [Test]
        public void Then_state_have_to_be_changed_to_stopped()
        {
            DownloadSlot.State.ShouldEqual(DownloadState.Stopped);
        }
    }

    public class And_Removing_download : When_working_with_download_slot
    {
        private bool removeCalled = false;

        protected override void Establish_context()
        {
            base.Establish_context();
            DownloadSlotViewModel.OnRemove += () => removeCalled = true;
        }

        protected override void Because_of()
        {
            DownloadSlotViewModel.Remove();
        }

        [Test]
        public void Then_remove_event_should_be_called()
        {
            removeCalled.ShouldEqual(true);
        }
    }
}
