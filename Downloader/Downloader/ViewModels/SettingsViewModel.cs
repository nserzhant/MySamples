using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using Downloader.MVVM;
using System.Windows.Forms;
using Downloader.Services;

namespace Downloader.ViewModels
{

    /// <summary>
    /// ViewModel which binds to Setting Control
    /// </summary>
    public interface ISettingsViewModel
    {
        bool IsDisplayed { get; set; }
        string ProxyIP { get; set; }
        int ProxyPort { get; set; }
        string SavingFolder { get; set; }
        int ThreadsCount { get; set; }
        bool UseProxy { get; set; }

        RelayCommand CloseSettingsCommand { get; }
        RelayCommand SaveSettingsCommand { get; }
        RelayCommand SelectSavePathCommand { get; }
    }

    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettings settings = null;
        private readonly IFileSystemManager folderSelector = null;
        private bool isDisplayed = false;

        public SettingsViewModel(ISettings settings, IFileSystemManager folderSelector)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (folderSelector == null)
                throw new ArgumentNullException("folderSelector");

            this.settings = settings;
            this.folderSelector = folderSelector;
            this.settings.Load();
        }

        #region Properties

        public bool IsDisplayed
        {
            get { return isDisplayed; }
            set
            {
                if (isDisplayed != value)
                {
                    this.isDisplayed = value;
                    NotifyPropertyChanged("IsDisplayed");
                }
            }
        }

        public string ProxyIP
        {
            get { return settings.ProxyIP; }
            set
            {
                if (settings.ProxyIP != value)
                {
                    settings.ProxyIP = value;
                    NotifyPropertyChanged("ProxyIP");
                }
            }
        }

        public int ProxyPort
        {
            get { return settings.ProxyPort; }
            set
            {
                if (settings.ProxyPort != value)
                {
                    settings.ProxyPort = value;
                    NotifyPropertyChanged("ProxyPort");
                }
            }
        }

        public int ThreadsCount
        {
            get { return settings.ThreadsCount - 1; }
            set
            {
                settings.ThreadsCount = value + 1;
                NotifyPropertyChanged("ThreadsCount");
            }
        }

        public string SavingFolder
        {
            get { return settings.SavingFolder; }
            set
            {
                if (settings.SavingFolder != value)
                {
                    settings.SavingFolder = value;
                    NotifyPropertyChanged("SavingFolder");
                }
            }
        }

        public bool UseProxy
        {
            get { return settings.UseProxy; }
            set
            {
                if (settings.UseProxy != value)
                {
                    settings.UseProxy = value;
                    NotifyPropertyChanged("UseProxy");
                }
            }
        }

        #endregion

        # region Commands

        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    settings.Save();
                });
            }
        }

        public RelayCommand SelectSavePathCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SavingFolder = folderSelector.SelectFolder(SavingFolder);
                });
            }
        }

        public RelayCommand CloseSettingsCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    IsDisplayed = false;
                });
            }
        }

        #endregion
    }
}
