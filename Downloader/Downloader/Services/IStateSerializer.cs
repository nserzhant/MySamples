using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;

namespace Downloader.Services
{
    /// <summary>
    /// Saves and loads state of the application when application closing or starting
    /// </summary>
    public interface IStateSerializer
    {
        void Save();

        /// <summary>
        /// Returns object which describes current application state
        /// </summary>
        DownloaderState CurrentState { get; }
    }
}
