using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using Downloader.Services;
using Moq;
using System.IO;
using Downloader.Models;
using NUnit.Framework;

namespace Downloader.Tests.Integration.Services
{
    [Category("BinaryStateSerializer")]
    public class When_working_with_binary_state_serializer : SpecBase
    {
        protected BinaryStateSerializer BinaryStateSerializer = null;
        protected Mock<ILogger> Logger = null;

        protected override void Establish_context()
        {
            base.Establish_context();

            Logger = new Mock<ILogger>();
            BinaryStateSerializer = new BinaryStateSerializer(Logger.Object);
        }
    }

    public class And_saving_and_restoring_state : When_working_with_binary_state_serializer
    {
        private readonly string uriInStateToSave = "some testing uri";
        private DownloaderState createdByDefaultState = null;
        private DownloaderState restoredState = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            if (File.Exists(BinaryStateSerializer.STATE_FILE_NAME))
            {
                File.Delete(BinaryStateSerializer.STATE_FILE_NAME);
            }

            createdByDefaultState = BinaryStateSerializer.CurrentState;
            createdByDefaultState.CurrentUri = uriInStateToSave;
        }

        protected override void Because_of()
        {
            //save to file
            BinaryStateSerializer.Save();
            //create new serializer, and get loaded state
            BinaryStateSerializer = new BinaryStateSerializer(Logger.Object);
            //save loaded state
            restoredState = BinaryStateSerializer.CurrentState;
        }

        [Test]
        public void Then_state_should_be_same()
        {
            restoredState.CurrentUri.ShouldEqual(uriInStateToSave);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            if (File.Exists(BinaryStateSerializer.STATE_FILE_NAME))
            {
                File.Delete(BinaryStateSerializer.STATE_FILE_NAME);
            }
        }
    }
}
