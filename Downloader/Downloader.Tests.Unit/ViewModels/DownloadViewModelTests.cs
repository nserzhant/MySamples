using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Downloader.ViewModels;
using Downloader.Models;
using Downloader.Services;
using Moq;
using NBehave.Spec.NUnit;

namespace Downloader.Tests.Unit.ViewModels
{
    [Category("DownloadViewModel")]
    public class When_working_with_download_view_model : SpecBase
    {
        private readonly string savingFolder = "C://some folder";
        private readonly string savingFileName = "savingFile";

        protected DownloadViewModel DownloadViewModel = null;
        protected string SavePath = null;

        protected Mock<IDownloadSlotsViewModel> DownloadSlotsViewModel = null;
        protected Mock<IDownloadModel> Download = null;
        protected Mock<IFileSystemManager> FileSystemManager = null;

        protected DownloadState LastSettingState;

        protected override void Establish_context()
        {
            base.Establish_context();
            DownloadSlotsViewModel = new Mock<IDownloadSlotsViewModel>();

            Download = new Mock<IDownloadModel>();
            Download.SetupGet(download => download.SaveFolderPath).Returns(savingFolder);
            Download.SetupGet(download => download.FileName).Returns(savingFileName);

            Download.SetupSet(download => download.State = It.IsAny<DownloadState>()).
                Callback<DownloadState>(state => LastSettingState = state);

            FileSystemManager = new Mock<IFileSystemManager>();
            DownloadViewModel = new DownloadViewModel(Download.Object, DownloadSlotsViewModel.Object, FileSystemManager.Object);

            SavePath = System.IO.Path.Combine(savingFolder, savingFileName);

        }
    }

    public class And_change_uri_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.ChangeUriCommand.Execute(null);
        }

        [Test]
        public void Then_download_in_slots_view_model_should_be_stopped()
        {
            DownloadSlotsViewModel.Verify(slots => slots.StopDownloading());
        }

        [Test]
        public void Then_download_in_slots_view_model_should_be_started()
        {
            DownloadSlotsViewModel.Verify(slots => slots.StartDownloading());
        }

        [Test]
        public void Then_last_setted_state_should_be_downloading()
        {
            LastSettingState.ShouldEqual(DownloadState.Downloading);
        }
    }

    public class And_continue_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.ContinueCommand.Execute(null);
        }

        [Test]
        public void Then_downloads_in_slots_view_model_should_be_started()
        {
            DownloadSlotsViewModel.Verify(slots => slots.StartDownloading());
        }

        [Test]
        public void Then_last_setted_state_should_be_downloading()
        {
            LastSettingState.ShouldEqual(DownloadState.Downloading);
        }
    }

    public class And_pause_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.PauseCommand.Execute(null);
        }

        [Test]
        public void Then_downloads_in_slots_view_model_should_be_stopped()
        {
            DownloadSlotsViewModel.Verify(slots => slots.StopDownloading());
        }

        [Test]
        public void Then_last_setted_state_should_be_downloading()
        {
            LastSettingState.ShouldEqual(DownloadState.Stopped);
        }
    }

    public class And_open_file_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.OpenFileCommand.Execute(null);
        }

        [Test]
        public void Then_open_file_of_file_system_manager_should_be_called()
        {
            FileSystemManager.Verify(fileSystemManager => fileSystemManager.OpenFile(SavePath));
        }
    }

    public class And_open_folder_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.OpenFolderCommand.Execute(null);
        }

        [Test]
        public void Then_open_file_of_file_system_manager_should_be_called()
        {
            FileSystemManager.Verify(fileSystemManager => fileSystemManager.OpenFolder(SavePath));
        }
    }

    public class And_remove_file_command_executed : When_working_with_download_view_model
    {
        protected override void Because_of()
        {
            DownloadViewModel.RemoveCommand.Execute(null);
        }

        [Test]
        public void Then_open_file_of_file_system_manager_should_be_called()
        {
            FileSystemManager.Verify(fileSystemManager => fileSystemManager.RevmoveFile(SavePath));
        }
    }

    public class And_remove_command_executed : When_working_with_download_view_model
    {
        private bool eventRaised = false;

        protected override void Because_of()
        {
            DownloadViewModel.OnRemove += () => { eventRaised = true; };
            DownloadViewModel.RemoveCommand.Execute(null);
        }

        [Test]
        public void Then_downloading_should_be_stopped()
        {
            DownloadSlotsViewModel.Verify(slots => slots.StopDownloading());
        }

        [Test]
        public void Then_downloaded_file_should_be_removed()
        {
            FileSystemManager.Verify(fileSystemManager => fileSystemManager.RevmoveFile(SavePath));
        }

        [Test]
        public void Then_removed_event_should_be_raised()
        {
            eventRaised.ShouldEqual(true);
        }
    }
}
