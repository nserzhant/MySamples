using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Downloader.Models;

namespace Downloader.Services
{
    /// <summary>
    /// Writes downloaded data
    /// </summary>
    public interface IFileStreamClient
    {
        /// <summary>
        /// Opens stream with seeking to specified position
        /// </summary>
        void Open(long offset);

        /// <summary>
        /// Wtites data block to stream
        /// </summary>
        /// <param name="buffer">Buffer contains data</param>
        /// <param name="count">Count of taking bytes from buffer</param>
        void Write(byte[] buffer, int count);

        /// <summary>
        /// Close stream
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Writes downloaded data to file
    /// </summary>
    public class FileStreamClient : IFileStreamClient
    {
        private readonly IFilePathSource filePathSource = null;
        private FileStream fileStream = null;

        public FileStreamClient(IFilePathSource filePathSource)
        {
            if (filePathSource == null)
                throw new ArgumentNullException("filePathSource");

            this.filePathSource = filePathSource;
        }

        public void Open(long offset)
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
            string filePath = Path.Combine(filePathSource.SaveFolderPath,filePathSource.FileName);
            fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            fileStream.Seek(offset, SeekOrigin.Begin);
        }

        public void Write(byte[] buffer, int count)
        {
            if (fileStream != null)
            {
                fileStream.Write(buffer, 0, count);
                fileStream.Flush();
            }
        }

        public void Close()
        {
            if (fileStream != null)
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }
        }
    }
}
