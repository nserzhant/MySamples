using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Downloader.Services
{
    /// <summary>
    /// Measures downloading speed
    /// </summary>
    public interface ISpeedMeasurer
    {
        /// <summary>
        /// Add new downloading bytes count to measurements
        /// </summary>
        void Add(int count);

        /// <summary>
        /// Returns current downloading speed
        /// </summary>
        double Speed { get; }
    }

    /// <summary>
    /// Speed measurer which uses LinkedList of actual (by time) measurements
    /// </summary>
    public class SpeedMeasurer : ISpeedMeasurer
    {
        private struct Measurement
        {
            public int Size;
            public DateTime Time;
        }

        private readonly TimeSpan checkingPeriod = TimeSpan.FromSeconds(5);
        private readonly LinkedList<Measurement> measurements = new LinkedList<Measurement>();

        private long currentDownloaded = 0;
        private long currentDropped = 0;
        private DateTime currentFrom = DateTime.Now;

        public double Speed
        {
            get
            {
                double durationInSec = (DateTime.Now - currentFrom).TotalSeconds;
                double downloaded = currentDownloaded - currentDropped;
                return durationInSec == 0 ? 0 :
                    downloaded / durationInSec;
            }
        }

        public void Add(int count)
        {
            currentDownloaded += count;
            Measurement measurement = new Measurement()
            {
                Size = count,
                Time = DateTime.Now
            };
            measurements.AddLast(measurement);
            while (measurements.First != measurements.Last && measurements.First.Next != measurements.Last &&
                (measurements.Last.Value.Time - measurements.First.Value.Time) > checkingPeriod)
            {
                currentDropped += measurements.First.Value.Size;
                currentFrom = measurements.First.Value.Time;
                measurements.RemoveFirst();
            }
        }

    }

}
