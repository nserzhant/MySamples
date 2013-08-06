using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.Services;
using Moq;
using Downloader.Models;
using Downloader.ViewModels;
using System.Threading;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace Downloader.Tests.Unit.Services
{
    [Category("DownloadSlotsViewModelAsyncFactory")]
    public class When_working_with_download_slots_view_model_factory : SpecBase
    {
        protected DownloadSlotsViewModelAsyncFactory DownloadSlotsViewModelAsyncFactory = null;

        protected Mock<IDownloadSlotsViewModelFactory> DownloadSlotsViewModelFactory = null;
        protected Mock<ILogger> Logger = null;

        protected override void Establish_context()
        {
            base.Establish_context();

            DownloadSlotsViewModelFactory = new Mock<IDownloadSlotsViewModelFactory>();
            Logger = new Mock<ILogger>();

            DownloadSlotsViewModelAsyncFactory = new DownloadSlotsViewModelAsyncFactory(DownloadSlotsViewModelFactory.Object, 
                Logger.Object);

            NotificationService.Notifier = null;
        }
    }
                
    public class And_create_download_stos_view_model_asyncronously : 
        When_working_with_download_slots_view_model_factory
    {
        private Download download = null;
        private bool createdIsRaised = false;
        private Download forDownloadCreated = null;
        private IDownloadSlotsViewModel viewModelCreated = null;
        private Mock<IDownloadSlotsViewModel> generatedViewModel = null;
        private ManualResetEvent resetEvent = new ManualResetEvent(false);

        protected override void Establish_context()
        {
            base.Establish_context();
            download = new Download("some uri");
            generatedViewModel = new Mock<IDownloadSlotsViewModel>();
            generatedViewModel.Setup(viewModel => viewModel.TryInitialize()).Returns(true);

            DownloadSlotsViewModelFactory.Setup(factory => factory.CreateDownloadSlotsViewModel(download)).
                Returns(generatedViewModel.Object);

            DownloadSlotsViewModelAsyncFactory.InitializationCompleted += (forDownload, viewModel) =>
                {
                    createdIsRaised = true;
                    forDownloadCreated = forDownload;
                    viewModelCreated = viewModel;
                    resetEvent.Set();
                };

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        protected override void Because_of()
        {
            DownloadSlotsViewModelAsyncFactory.TryInitializeDownloadSlotsViewModelAsync(download);
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(1000));
        }

        [Test]
        public void Then_created_event_should_be_raised()
        {
            createdIsRaised.ShouldEqual(true);
        }

        [Test]
        public void Then_appropriate_download_should_be_returned()
        {
            forDownloadCreated.ShouldEqual(download);
        }

        [Test]
        public void Then_not_null_view_model_should_be_returned()
        {
            viewModelCreated.ShouldNotBeNull();
        }
    }
}
