using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Models
{
    /// <summary>
    /// Network - related information about downloading content
    /// </summary>
    public interface IUriSource
    {
        /// <summary>
        /// Gets network path to the downloaded source
        /// </summary>
        string Uri { get; }
    }

    /// <summary>
    /// Filesystem related information for downloading content
    /// </summary>
    public interface IFilePathSource
    {
        /// <summary>
        /// Gets or sets forlder where file have to be saved
        /// </summary>
        string SaveFolderPath { get; }

        /// <summary>
        /// Gets or sets downloading file name
        /// </summary>
        string FileName { get; }
    }

    /// <summary>
    /// DataSource for downloading content components
    /// </summary>
    public interface IDownloadSlotsModel : IUriSource, IFilePathSource
    {
        /// <summary>
        /// Gets downloading parts info for whoole download
        /// </summary>
        List<DownloadSlot> DownloadSlots { get; }

        /// <summary>
        /// Gets or sets downloading file name
        /// </summary>
        string FileName { get; set; }
    }

    /// <summary>
    /// DataSource for managing downliading/saving source's components
    /// </summary>
    public interface IDownloadModel : IFilePathSource
    {
        /// <summary>
        /// Gets or sets network path to the downloaded source
        /// </summary>
        string Uri { get; set; }

        /// <summary>
        /// Gets or sets saving file name
        /// </summary>
        string FileName { get; set; }
        /// <summary>
        /// Gets or sets current download state
        /// </summary>
        DownloadState State { get; set; }
    }

    /// <summary>
    /// Represents state of single download
    /// </summary>
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
