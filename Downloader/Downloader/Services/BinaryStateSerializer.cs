using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace Downloader.Services
{
    /// <summary>
    /// Saves downloads state into binary formatted file
    /// </summary>
    public class BinaryStateSerializer : IStateSerializer
    {
        public static readonly string STATE_FILE_NAME = "downloads.bin";
        private NetDataContractSerializer netDataContractSerializer = null;
        private readonly ILogger logger = null;

        public BinaryStateSerializer(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
            netDataContractSerializer = new NetDataContractSerializer();
            load();
        }

        public DownloaderState CurrentState
        {
            get;
            private set;
        }

        public void Save()
        {
            try
            {
                if (File.Exists(STATE_FILE_NAME))
                {
                    File.Delete(STATE_FILE_NAME);
                }

                using (Stream fileStr = File.Create(STATE_FILE_NAME))
                {
                    netDataContractSerializer.Serialize(fileStr, CurrentState);
                    fileStr.Flush();
                    fileStr.Close();
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }

        private void load()
        {
            DownloaderState deserialized = null;
            if (File.Exists(STATE_FILE_NAME))
            {
                try
                {

                    using (var stream = File.OpenRead(STATE_FILE_NAME))
                    {
                        deserialized = netDataContractSerializer.Deserialize(stream)
                            as DownloaderState;
                        stream.Close();
                    }
                }
                catch (XmlException e)
                {
                    deserialized = null;
                    logger.LogException(e);
                }
            }
            if (deserialized != null)
            {
                CurrentState = deserialized;
            }
            else
            {
                CurrentState = new DownloaderState();
            }
        }

    }
}
