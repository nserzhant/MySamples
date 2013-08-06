using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader
{
    public enum DownloadState
    {
        Initializing,
        Stopped,
        Downloaded,
        Terminated,
        Downloading
    }
}
