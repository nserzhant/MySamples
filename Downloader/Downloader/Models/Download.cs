using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Models
{
    public interface IUriSource
    {
        string Uri { get; }
    }

    public interface IFilePathSource
    {
        string SaveFolderPath { get; }
        string FileName { get; }
    }

    public interface IDownloadSlotsModel : IUriSource, IFilePathSource
    {
        List<DownloadSlot> DownloadSlots { get; }
        string FileName { get; set; }
    }

    public interface IDownloadModel
    {
        string Uri { get; set; }
        string SaveFolderPath { get; }
        string FileName { get; set; }
        DownloadState State { get; set; }
    }

    public class Download : IDownloadModel, IDownloadSlotsModel
    {
        #region IDownloadModel members

        public string Uri { get; set; }

        public string SaveFolderPath { get; set; }

        public string FileName { get; set; }        

        public DownloadState State { get; set; }

        #endregion

        public List<DownloadSlot> DownloadSlots { get; set; }

        public Download()
            : this(null, null)
        {
        }

        public Download(string Uri, List<DownloadSlot> downloadSlots = null)
        {
            this.Uri = Uri;

            if (downloadSlots != null)
            {
                this.DownloadSlots = downloadSlots;
            }
            else
            {
                this.DownloadSlots = new List<DownloadSlot>();
            }
        }

    }
}
