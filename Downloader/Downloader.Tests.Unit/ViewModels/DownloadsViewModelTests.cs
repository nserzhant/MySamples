using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Downloader.ViewModels;
using Downloader.Models;
using Downloader.Services;
using Moq;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace Downloader.Tests.Unit.ViewModels
{
    [Category("DownloadsViewModel")]
    public class When_working_with_downloads_view_model : SpecBase
    {
        protected DownloadsViewModel DownloadsViewModel = null;
        protected IList<Download> Downloads = null;
        protected Mock<IDownloadViewModelFactory> DownloadViewModelFactory = null;
        protected Mock<IDownloadSlotsViewModelAsyncFactory> DownloadSlotsFactory = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            Downloads = this.createDownloads();
            var fileSystemManagerMock = new Mock<IFileSystemManager>().Object;
            DownloadViewModelFactory = new Mock<IDownloadViewModelFactory>();
            DownloadSlotsFactory = new Mock<IDownloadSlotsViewModelAsyncFactory>();
            DownloadsViewModel = new DownloadsViewModel(Downloads, DownloadViewModelFactory.Object, DownloadSlotsFactory.Object);
        }

        private IList<Download> createDownloads()
        {
            return new List<Download>();
        }
    }

    public class And_new_download_added : When_working_with_downloads_view_model
    {
        private readonly string someUri = "some uri";
        private readonly string somePath = "C://savingFolder";

        Mock<IDownloadViewModel> generatedViewModel = null;

        protected override void Establish_context()
        {
            base.Establish_context();

            Download returnsDownload = null;
            generatedViewModel = new Mock<IDownloadViewModel>();

            DownloadSlotsFactory.Setup(factory => factory.TryInitializeDownloadSlotsViewModelAsync(It.IsAny<Download>())).
                Callback<Download>(download =>
                {
                    returnsDownload = download;
                    DownloadSlotsFactory.Raise(factory => factory.InitializationCompleted += null,
                        returnsDownload, new Mock<IDownloadSlotsViewModel>().Object);
                });

            DownloadViewModelFactory.Setup(factory => factory.CreateDownloadViewModel(It.IsAny<Download>(), It.IsAny<IDownloadSlotsViewModel>()))
                .Returns(generatedViewModel.Object);
                
        }

        protected override void Because_of()
        {
            DownloadsViewModel.AddDownload(someUri, somePath);
        }

        [Test]
        public void Then_initialization_of_new_download_slots_view_model_with_right_Uri_and_path_should_be_started()
        {
            DownloadSlotsFactory.Verify(factory => factory.TryInitializeDownloadSlotsViewModelAsync(
                It.Is<Download>(download => download.SaveFolderPath == somePath && download.Uri == someUri)));
        }

        [Test]
        public void Then_new_view_model_item_should_be_added_to_items_collection()
        {
            DownloadsViewModel.Items.Count().ShouldEqual(1);
        }

        [Test]
        public void Then_start_method_of_new_view_model_should_be_executed()
        {
            generatedViewModel.Verify(vm => vm.Start());
        }
    }
}
