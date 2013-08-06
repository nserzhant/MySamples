using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Services
{
    /// <summary>
    /// Creates SpeedMeasurer's instances
    /// </summary>
    public interface ISpeedMeasurerFactory
    {
        ISpeedMeasurer CreateMeasurer();
    }

    public class SpeedMeasurerFactory : ISpeedMeasurerFactory
    {
        public ISpeedMeasurer CreateMeasurer()
        {
            return new SpeedMeasurer();
        }
    }
}
