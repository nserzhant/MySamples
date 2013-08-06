using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using Downloader.Services;
using System.Threading;
using NUnit.Framework;

namespace Downloader.Tests.Integration.Services
{
    [Category("SpeedMeasurer")]
    public class When_working_with_speed_measurer : SpecBase
    {
        protected SpeedMeasurer SpeedMeasurer = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            SpeedMeasurer = new SpeedMeasurer();
        }
    }

    public class And_measuring_speed : When_working_with_speed_measurer
    {
        private readonly int iterations = 10;
        private readonly int countPerMeasure = 5000;

        private DateTime timeFrom;
        private DateTime timeTo;
        private double expectedSpeed = 0;
        private double currentSpeed;

        protected override void Because_of()
        {
            timeFrom = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                SpeedMeasurer.Add(countPerMeasure);
                Thread.Sleep(200);
            }
            timeTo = DateTime.Now;

            expectedSpeed = countPerMeasure * iterations * 1000.0 / (timeTo - timeFrom).TotalMilliseconds;
            currentSpeed = SpeedMeasurer.Speed;
        }

        [Test]
        public void Then_a_right_speed_with_appropriate_precision_should_be_measured()
        {
            Math.Round(currentSpeed, 3).ShouldEqual(Math.Round(expectedSpeed, 3));
        }
    }
}
