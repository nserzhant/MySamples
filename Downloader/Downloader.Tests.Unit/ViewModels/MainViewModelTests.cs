using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Downloader.Services;
using Moq;
using Downloader.Models;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace Downloader.Tests.Unit.ViewModels
{
    [Category("MainViewModel")]
    public class When_working_with_main_view_model : SpecBase
    {
        protected MainViewModel MainViewModel = null;
        protected Mock<IMainModel> MainModel = null;
        protected Mock<ISettingsViewModel> SettingsViewModel = null;
        protected Mock<ILogger> Logger = null;
        protected Mock<IDownloadsViewModel> DownloadsViewModel = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            MainModel = new Mock<IMainModel>();
            SettingsViewModel = new Mock<ISettingsViewModel>();
            Logger = new Mock<ILogger>();
            DownloadsViewModel = new Mock<IDownloadsViewModel>();
            MainViewModel = new MainViewModel(MainModel.Object, SettingsViewModel.Object, DownloadsViewModel.Object, Logger.Object);
        }
    }

    public class And_add_command_executed : When_working_with_main_view_model
    {
        private readonly string savingFolderPath = "somePath";
        private readonly string downloadUri = "someUri";

        protected override void Establish_context()
        {
            base.Establish_context();
            SettingsViewModel.Setup(settings => settings.SavingFolder).Returns(savingFolderPath);
            MainModel.Setup(model => model.CurrentUri).Returns(downloadUri);
        }

        protected override void Because_of()
        {
            MainViewModel.AddDownloadCommand.Execute(null);
        }

        [Test]
        public void Then_Add_download_on_downloadsViewModel_shold_be_called_with_current_saving_folder_and_uri()
        {
            DownloadsViewModel.Verify(downloadsViewModel => downloadsViewModel.AddDownload(downloadUri, savingFolderPath));
        }
    }

    public class And_clear_download_command_executed : When_working_with_main_view_model
    {
        protected override void Establish_context()
        {
            base.Establish_context();
            MainModel.Setup(context => context.CurrentUri).Returns("not empty value");
        }

        protected override void Because_of()
        {
            MainViewModel.ClearCommand.Execute(null);
        }

        [Test]
        public void Current_uri_should_be_cleared()
        {
            MainModel.VerifySet(model => model.CurrentUri = "");
        }
    }

    public class And_show_settings_command_executed : When_working_with_main_view_model
    {
        protected override void Establish_context()
        {
            base.Establish_context();
        }

        protected override void Because_of()
        {
            MainViewModel.ShowSettingsCommand.Execute(null);
        }

        [Test]
        public void Then_IsDisplayed_Property_of_settings_view_model_should_be_set_to_true()
        {
            SettingsViewModel.VerifySet(settings => settings.IsDisplayed = true);
        }
    }
}
