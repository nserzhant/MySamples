using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Downloader.Models
{
    /// <summary>
    /// Represents state of current application settings
    /// </summary>
    public interface ISettings : ISaveFileSettings, INetworkSettings
    {
        /// <summary>
        /// Load current application settings
        /// </summary>
        void Load();

        /// <summary>
        /// Saves current application settings
        /// </summary>
        void Save();
    }

    /// <summary>
    /// File system related settings
    /// </summary>
    public interface ISaveFileSettings
    {
        /// <summary>
        /// Gets or sets path where new files would be saved
        /// </summary>
        string SavingFolder { get; set; }
    }

    /// <summary>
    /// Network related settings
    /// </summary>
    public interface INetworkSettings
    {
        /// <summary>
        /// Gets or sets IP address of proxy server which will be using for connection to network 
        /// </summary>
        string ProxyIP { get; set; }
        /// <summary>
        /// Gets or sets port for Proxy server
        /// </summary>
        int ProxyPort { get; set; }
        /// <summary>
        /// Gets or sets maximum numbers of threads content downloads simultaneously for a single file
        /// </summary>
        int ThreadsCount { get; set; }
        /// <summary>
        /// Gets or sets should proxy settings be taking into account
        /// </summary>
        bool UseProxy { get; set; }
        /// <summary>
        /// Gets or sets maximum bytes count for single block downloaded from network source
        /// </summary>
        int BlockSize { get; set; }
    }

    /// <summary>
    /// Used for saving application settings in User-scoped application settings
    /// </summary>
    public class AppConfigSettings : ISettings
    {
        private readonly int DEFAULT_BLOCK_SIZE = 10000;
        private readonly string DEFAUL_SAVE_DIRECTORY = "downloads";

        public string ProxyIP
        {
            get { return Properties.Settings.Default.ProxyIP; }
            set { Properties.Settings.Default.ProxyIP = value; }
        }
        public int ProxyPort 
        {
            get { return Properties.Settings.Default.ProxyPort; }
            set { Properties.Settings.Default.ProxyPort = value; }
        }
        public int ThreadsCount
        {
            get { return Properties.Settings.Default.ThreadsCount; }
            set { Properties.Settings.Default.ThreadsCount = value; }
        }
        public string SavingFolder
        {
            get { return Properties.Settings.Default.SavingFolder; }
            set { Properties.Settings.Default.SavingFolder = value; }
        }
        public bool UseProxy
        {
            get { return Properties.Settings.Default.UseProxy; }
            set { Properties.Settings.Default.UseProxy = value; }
        }
        public int BlockSize
        {
            get { return Properties.Settings.Default.BlockSize; }
            set { Properties.Settings.Default.BlockSize = value; }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public void Load()
        {
            if (BlockSize <= 0)
            {
                BlockSize = DEFAULT_BLOCK_SIZE;
            }
            if (ThreadsCount <= 0)
            {
                ThreadsCount = 1;
            }
            if (string.IsNullOrEmpty(SavingFolder))
            {
                if (!Directory.Exists(DEFAUL_SAVE_DIRECTORY))
                {
                    Directory.CreateDirectory(DEFAUL_SAVE_DIRECTORY);
                }
                SavingFolder = DEFAUL_SAVE_DIRECTORY;
            }
        }
    }
}
