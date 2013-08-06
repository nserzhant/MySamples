using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;

namespace Downloader.Services
{
    /// <summary>
    /// Creates instances of IFileStreamClient
    /// </summary>
    public interface IFileStreamClientFactory
    {
        IFileStreamClient CreateFileStreamClient(IFilePathSource filePathSource);
    }

    public class FileStreamClientFactory : IFileStreamClientFactory
    {
        public IFileStreamClient CreateFileStreamClient(IFilePathSource filePathSource)
        {
            return new FileStreamClient(filePathSource);
        }
    }
}
