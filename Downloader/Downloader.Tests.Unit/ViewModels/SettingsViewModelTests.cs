using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Moq;
using Downloader.Models;
using Downloader.Services;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace Downloader.Tests.Unit.ViewModels
{
    [Category("SettingsViewModel")]
    public class When_working_with_settings_view_model : SpecBase
    {
        protected SettingsViewModel SettingsViewModel = null;
        protected Mock<ISettings> SettingMock = null;
        protected Mock<IFileSystemManager> FileSystemManagerMock = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            SettingMock = new Mock<ISettings>();
            FileSystemManagerMock = new Mock<IFileSystemManager>();
            SettingsViewModel = new SettingsViewModel(SettingMock.Object, FileSystemManagerMock.Object);
        }
    }

    public class And_save_settings_command_executed : When_working_with_settings_view_model
    {
        protected override void Because_of()
        {
            SettingsViewModel.SaveSettingsCommand.Execute(null);
        }

        [Test]
        public void Then_save_method_of_the_settings_model_have_to_be_called()
        {
            SettingMock.Verify(sett => sett.Save());
        }
    }

    public class And_select_save_path_command_executed : When_working_with_settings_view_model
    {
        private readonly string selectedFolder = "C://someFolder";

        protected override void Establish_context()
        {
            base.Establish_context();
            base.FileSystemManagerMock.Setup(manager => manager.SelectFolder(null)).Returns(selectedFolder);
        }

        protected override void Because_of()
        {
            SettingsViewModel.SelectSavePathCommand.Execute(null);
        }

        [Test]
        public void Then_selected_folder_for_save_path_setting_have_to_be_set()
        {
            SettingMock.VerifySet(setting => setting.SavingFolder = selectedFolder);
        }

    }

    public class And_close_command_executed : When_working_with_settings_view_model
    {
        protected override void Establish_context()
        {
            base.Establish_context();
            SettingsViewModel.IsDisplayed = true;
        }

        protected override void Because_of()
        {
            SettingsViewModel.CloseSettingsCommand.Execute(null);
        }

        [Test]
        public void Then_SettinngsViewModel_have_not_to_be_more_visible()
        {
            SettingsViewModel.IsDisplayed.ShouldEqual(false);
        }
    }
}
