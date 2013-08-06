using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Models
{
    public interface IMainModel
    {
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
