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
    /// Represents the whoole dataContext of application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IMainModel mainModel = null;
        private readonly ISettingsViewModel settingsViewModel = null;
        private readonly IDownloadsViewModel downloadsViewModel = null;
        private readonly ILogger logger = null;

        public MainViewModel(IMainModel mainModel, ISettingsViewModel settingsViewModel,
            IDownloadsViewModel downloadsViewModel,
            ILogger logger)
        {
            if (mainModel == null)
                throw new ArgumentNullException("mainModel");
            if (settingsViewModel == null)
                throw new ArgumentNullException("settingsViewModel");
            if (downloadsViewModel == null)
                throw new ArgumentNullException("downloadsViewModel");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.mainModel = mainModel;
            this.settingsViewModel = settingsViewModel;
            this.downloadsViewModel = downloadsViewModel;
            this.logger = logger;
        }

        #region Properties

        public ISettingsViewModel SettingsViewModel
        {
            get { return settingsViewModel; }
        }

        public IDownloadsViewModel DownloadsViewModel
        {
            get { return downloadsViewModel; }
        }

        public string CurrentUri
        {
            get { return mainModel.CurrentUri; }
            set
            {
                if (mainModel.CurrentUri != value)
                {
                    mainModel.CurrentUri = value;
                    NotifyPropertyChanged("CurrentUri");
                }
            }
        }

        #endregion

        #region Commands
        public RelayCommand AddDownloadCommand
        {
            get { return new RelayCommand(addDownload); }
        }

        public RelayCommand ClearCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    CurrentUri = "";
                });
            }
        }

        public RelayCommand ShowSettingsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SettingsViewModel.IsDisplayed = true;
                }, (obj) => !SettingsViewModel.IsDisplayed);
            }
        } 
        #endregion

        #region Methods
        private void addDownload(object item)
        {
            try
            {
                string savingFolder = settingsViewModel.SavingFolder;
                this.DownloadsViewModel.AddDownload(CurrentUri, savingFolder);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                NotificationService.Alert(Properties.Resources.DownloadStartFailedMessage,
                    Properties.Resources.DownloadingFailedHeader);
            }
        } 
        #endregion
    }
}
