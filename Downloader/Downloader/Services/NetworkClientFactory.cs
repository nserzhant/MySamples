using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Models;

namespace Downloader.Services
{
    /// <summary>
    /// Creates a new instances of NetworkClient
    /// </summary>
    public interface INetworkClientFactory
    {
        INetworkClient CreateClient(IUriSource uriSource);
    }

    public class NetworkClientFactory : INetworkClientFactory
    {
        private readonly INetworkSettings networkSettings = null;
        private readonly ISpeedMeasurerFactory speedMeasurerFactory = null;
        private readonly ILogger logger = null;

        public NetworkClientFactory(INetworkSettings networkSettings,
            ISpeedMeasurerFactory speedMeasurerFactory,
            ILogger logger)
        {
            if (networkSettings == null)
                throw new ArgumentNullException("networkSettings");
            if (speedMeasurerFactory == null)
                throw new ArgumentNullException("speedMeasurerFactory");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.networkSettings = networkSettings;
            this.speedMeasurerFactory = speedMeasurerFactory;
            this.logger = logger;
        }

        public INetworkClient CreateClient(IUriSource uriSource)
        {
            return new NetworkClient(networkSettings, uriSource,
                speedMeasurerFactory.CreateMeasurer(), logger);
        }
    }
}
