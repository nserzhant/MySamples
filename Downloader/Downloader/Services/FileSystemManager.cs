using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Downloader.Services
{
    /// <summary>
    /// Provides user interaction with file system
    /// </summary>
    public interface IFileSystemManager
    {
        string SelectFolder(string defaultValue);

        void OpenFolder(string filePath);

        void OpenFile(string filePath);

        void RevmoveFile(string filePath);

        /// <summary>
        /// Rename file if it ends with ".tmp" (by removing ".tmp")
        /// </summary>
        /// <returns>File name of renamed file</returns>
        string RenameTempFile(string filePath);
    }

    public class FileSystemManager : IFileSystemManager
    {
        private ILogger logger = null;

        public FileSystemManager(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public string SelectFolder(string defaultValue)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath + @"\";
            }
            return defaultValue;
        }

        public void OpenFolder(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directory))
            {
                Process.Start(directory);
            }
            else
            {
                NotificationService.Alert(Properties.Resources.FileMissedMessage, "");
            }
        }

        public void OpenFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(filePath);
                }
                else
                {
                    NotificationService.Alert(Properties.Resources.FileMissedMessage, "");
                }
            }
            catch(Exception e)
            {
                logger.LogException(e);
                NotificationService.Alert(Properties.Resources.AppMissedMessage,
                    Properties.Resources.AppMissedHeader);
            }
        }

        public void RevmoveFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                NotificationService.Alert(Properties.Resources.FileMissedMessage, "");
            }
        }

        public string RenameTempFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                NotificationService.Alert(Properties.Resources.FileMissedMessage, "");
            }

            if (!filePath.EndsWith(".tmp"))
            {
                return filePath;
            }

            string newPath = filePath.Substring(0, filePath.Length - 4);
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            File.Move(filePath, newPath);
            return Path.GetFileName(newPath);
        }
    }
}
