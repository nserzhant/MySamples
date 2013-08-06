using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Downloader.Models;
using System.Threading.Tasks;
using System.Threading;

namespace Downloader.Services
{
    /// <summary>
    /// Provides interaction with remote data sources
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// Raises when new portion data is downloaded from network stream
        /// </summary>
        event Action<byte[], int> OnBlockDownloaded;

        /// <summary>
        /// Notify when error occured while downloading data
        /// </summary>
        event Action OnError;

        /// <summary>
        /// Notify when downloading starts after error occured
        /// </summary>
        event Action OnRestore;

        /// <summary>
        /// Raises when downloading comleted
        /// </summary>
        event Action OnComplete;

        /// <summary>
        /// Starts downloading in separate thread
        /// </summary>
        /// <param name="from">Position from where to download</param>
        /// <param name="to">Position up to which download</param>
        void StartDownloading(long from, long to);

        /// <summary>
        /// Stops downloading
        /// </summary>
        void StopDownloading();

        /// <summary>
        /// Get possible counts of simultaneous downloads of the same source
        /// </summary>
        int GetAvialableDownloads();

        /// <summary>
        /// Get information about downloading content
        /// </summary>
        /// <returns></returns>
        ContentProperties GetContentProperties();

        /// <summary>
        /// Returns current downloading speed
        /// </summary>
        double CurrentSpeed { get; }
    }

    public class NetworkClient : INetworkClient
    {
        private readonly int WAIT_FOR_CONNECTION_TIMEOUT = 10000;
        public event Action<byte[], int> OnBlockDownloaded;
        public event Action OnError;
        public event Action OnRestore;
        public event Action OnComplete;

        private readonly TaskFactory defaultTaskFactory = null;
        private Task downloadingTask = null;
        private long from = 0;
        private long to = 0;
        private bool canContinue = true;

        private readonly INetworkSettings networkSettings = null;
        private readonly IUriSource uriSource = null;
        private readonly ISpeedMeasurer speedMeasurer = null;
        private readonly ILogger logger = null;

        public NetworkClient(INetworkSettings settings, IUriSource uriSource,
            ISpeedMeasurer speedMeasurer,
            ILogger logger)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (uriSource == null)
                throw new ArgumentNullException("uriSource");
            if (speedMeasurer == null)
                throw new ArgumentNullException("speedMeasurer");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.networkSettings = settings;
            this.uriSource = uriSource;
            this.speedMeasurer = speedMeasurer;
            this.logger = logger;
            this.defaultTaskFactory = new TaskFactory(TaskScheduler.Default);
        }

        #region Properties
        public double CurrentSpeed
        {
            get
            {
                return speedMeasurer.Speed;
            }
        } 
        #endregion

        #region Methods

        public void StartDownloading(long from, long to)
        {
            canContinue = true;
            if (downloadingTask != null)
            {
                if (downloadingTask.Status != TaskStatus.RanToCompletion &&
                    downloadingTask.Status != TaskStatus.Faulted &&
                    downloadingTask.Status != TaskStatus.Canceled)
                {
                    downloadingTask.Wait();
                }
                downloadingTask.Dispose();
            }

            downloadingTask = defaultTaskFactory.StartNew(() => startDownloading(from, to));
            downloadingTask.ContinueWith((task) =>
                {
                    var exception = task.Exception;
                    logger.LogException(exception);
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public void StopDownloading()
        {
            canContinue = false;
            if (downloadingTask != null && downloadingTask.Status == TaskStatus.Running)
            {
                downloadingTask.Wait();
            }
        }

        public int GetAvialableDownloads()
        {
            int avialableCount = 0;
            List<HttpWebResponse> responseList = new List<HttpWebResponse>();
            List<Stream> responseStreamList = new List<Stream>();
            try
            {
                for (int i = 0; i < networkSettings.ThreadsCount; i++)
                {
                    HttpWebResponse response = createResponse(this.uriSource.Uri, 1);
                    if (response == null)
                    {
                        return i;
                    }
                    if (response.Headers["Accept-Ranges"] == null)
                    {
                        return 1;
                    }
                    responseList.Add(response);
                    responseStreamList.Add(response.GetResponseStream());
                    avialableCount++;
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
                avialableCount = 0;
            }
            finally
            {
                foreach (HttpWebResponse resp in responseList) resp.Close();
                foreach (Stream stream in responseStreamList) stream.Dispose();
            }
            return avialableCount;
        }

        public ContentProperties GetContentProperties()
        {
            ContentProperties properties = new ContentProperties();
            long contentSize = 0;
            string fileName = string.Empty;
            using (HttpWebResponse response = createResponse(this.uriSource.Uri, 0))
            {
                if (response == null)
                {
                    return properties;
                }
                contentSize = response.Headers["Content-Length"] == null ? 0 :
                    long.Parse(response.Headers["Content-Length"]);

                string uri = this.uriSource.Uri;
                if (!response.ResponseUri.AbsoluteUri.Equals(uri))
                {
                    uri = response.ResponseUri.ToString();
                }
                fileName = Path.GetFileName(uri);
                response.Close();
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName + ".tmp";
            }
            properties.FileName = fileName;
            properties.FileSize = contentSize;
            return properties;
        }


        private void startDownloading(long from, long to)
        {
            this.from = from;
            this.to = to;
            bool downloaded = false;
            string uri = uriSource.Uri;

            Stream stream = this.createDownloadingStream(uri, from);
            if (stream == null)
            {
                onError();
            }

            do
            {
                while (stream == null && canContinue)
                {
                    stream = this.createDownloadingStream(uri, this.from);
                    if (stream != null)
                    {
                        onRestore();
                    }
                }

                if (!(downloaded = this.tryDownload(stream)))
                {
                    onError();
                    stream = null;
                }
            }
            while (!downloaded && canContinue);
        }

        private bool tryDownload(Stream stream)
        {
            try
            {
                long toDownload = to - from;
                using (stream)
                {
                    if (stream != null)
                    {

                        byte[] buffer = new byte[networkSettings.BlockSize];
                        int count = 1;
                        while ((toDownload > 0 || (to == 0 && count > 0))
                            && canContinue)
                        {
                            count = stream.Read(buffer, 0, buffer.Length);
                            toDownload -= count;
                            long sizeToWrite = (toDownload <= 0 && to > 0) ? count + toDownload : count;
                            from += sizeToWrite;
                            onBlockDownloaded(buffer, (int)sizeToWrite);
                        }
                        if (canContinue)
                        {
                            onColmplete();
                        }
                        stream.Close();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return false;
            }
            return true;
        }

        private Stream createDownloadingStream(string uri, long from)
        {
            HttpWebResponse response = createResponse(uri, from);
            if (response == null)
            {
                return null;
            }
            //If server doesn't support seeking and we try to get data from a specified position
            if (response.Headers["Accept-Ranges"] == null && from > 0)
            {
                response.Close();
                return null;
            }
            return response.GetResponseStream();
        }

        private HttpWebResponse createResponse(string uri, long offset)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Timeout = WAIT_FOR_CONNECTION_TIMEOUT;
            request.AllowAutoRedirect = true;
            if (networkSettings.UseProxy)
            {
                request.Proxy = new WebProxy(networkSettings.ProxyIP, networkSettings.ProxyPort);
            }
            if (offset > 0)
            {
                request.AddRange(offset);
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)//hack for some sites
            {
                logger.LogException(ex);
                response = (HttpWebResponse)ex.Response;
            }
            return response;
        }

        /// <summary>
        /// Executes events asyncronously, for preventing from deadlocks
        /// </summary>
        /// <param name="action">Delegate for target event</param>
        private void executeSafeAsync(Action action)
        {
            this.defaultTaskFactory.StartNew(() =>
            {
                action();
            }).ContinueWith(
                (task) =>
                {
                    var exception = task.Exception;
                    logger.LogException(exception);
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private void onBlockDownloaded(byte[] buffer, int count)
        {
            this.speedMeasurer.Add(count);
            if (OnBlockDownloaded != null)
            {
                OnBlockDownloaded(buffer, count);
            }
        }

        private void onColmplete()
        {
            if (OnComplete != null)
            {
                executeSafeAsync(OnComplete);
            }
        }

        private void onError()
        {
            if (OnError != null)
            {
                executeSafeAsync(OnError);
            }
        }

        private void onRestore()
        {
            if (OnRestore != null)
            {
                executeSafeAsync(OnRestore);
            }
        } 
        #endregion
    }
}
