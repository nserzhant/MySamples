using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Models
{
    /// <summary>
    /// Represent state of root component itself
    /// </summary>
    public interface IMainModel
    {
        /// <summary>
        /// Gets or sets network source for new file downloading
        /// </summary>
        string CurrentUri { get; set; }
    }

    public class DownloaderState : IMainModel
    {
        public DownloaderState()
        {
            Downloads = new List<Download>();
        }

        public string CurrentUri { get; set; }
        
        public List<Download> Downloads { get; set; }
    }
}
