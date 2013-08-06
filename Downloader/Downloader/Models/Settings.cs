using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Downloader.Models
{
    public interface ISettings : ISaveFileSettings, INetworkSettings
    {
        void Load();
        void Save();
    }

    public interface ISaveFileSettings
    {
        string SavingFolder { get; set; }
    }

    public interface INetworkSettings
    {
        string ProxyIP { get; set; }
        int ProxyPort { get; set; }
        int ThreadsCount { get; set; }
        bool UseProxy { get; set; }
        int BlockSize { get; set; }
    }

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
