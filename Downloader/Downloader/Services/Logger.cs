using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Downloader.Services
{
    public interface ILogger
    {
        void LogException(Exception e);
    }

    public class Logger : ILogger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        public void LogException(Exception e)
        {
            log.Error(e);
        }
    }
}
