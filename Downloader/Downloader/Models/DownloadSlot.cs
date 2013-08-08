using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Models
{
    /// <summary>
    /// Represent state of single downloading content block
    /// </summary>
    public class DownloadSlot
    {
        /// <summary>
        /// Downloading part state
        /// </summary>
        public DownloadState State { get; set; }

        /// <summary>
        /// Start position of the downloading part
        /// </summary>
        public long StartPosition { get; set; }

        /// <summary>
        /// Finish position of the downloading part
        /// </summary>
        public long FinishPosition { get; set; }

        /// <summary>
        /// Current position in the downloading part
        /// </summary>
        public long CurrentPosition { get; set; }
    }
}
