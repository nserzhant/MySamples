using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using NUnit.Framework;
using System.IO;
using System.Net;

namespace WebService.Tests.Integration
{
    [Category("WebServiceManager")]
	public class When_working_with_WebServiceManager:SpecBase
	{
        protected readonly string EndpointAddress = @"http://localhost:4490/";
        private readonly string contentFolderPath = "content";
        protected WebServiceManager WebServiceManager = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            WebServiceManager = new WebServiceManager(contentFolderPath, "");
            createContentDirectory();
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            dropContentDirectory();
        }

        private void createContentDirectory()
        {
            if (!Directory.Exists(contentFolderPath))
            {
                Directory.CreateDirectory(contentFolderPath);
            }
        }

        private void dropContentDirectory()
        {
            if (Directory.Exists(contentFolderPath))
            {
                Directory.Delete(contentFolderPath, true);
            }
        }    
	}

    public class And_starting_web_service : When_working_with_WebServiceManager
    {
        private string errorMessage;

        protected override void Because_of()
        {
            WebServiceManager.TryStart(EndpointAddress, out errorMessage);
        }

        [Test]
        public void Then_service_without_errors_should_be_started()
        {
            errorMessage.ShouldBeEmpty();
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            WebServiceManager.ShootDown();
        }
    }

    public class When_working_with_started_WebServiceManager : When_working_with_WebServiceManager
    {
        protected string Request = "";

        protected override void Establish_context()
        {
            string errorMessage = "";
            base.Establish_context();
            WebServiceManager.TryStart(EndpointAddress, out errorMessage);
            Request = EndpointAddress + "Filelist";
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            WebServiceManager.ShootDown();
        }
    }

    public class And_getting_sending_some_web_request_to_hosted_service : When_working_with_started_WebServiceManager
    {
        private byte[] data = null;

        protected override void Because_of()
        {
            WebClient webClient = new WebClient();
            data = webClient.DownloadData(Request);
        }

        [Test]
        public void Then_some_data_should_be_returned()
        {
            data.ShouldNotBeNull();
        }
    }

    public class And_getting_sending_some_web_request_to_hosted_service_after_service_stopped :
        When_working_with_started_WebServiceManager
    {
        private byte[] data = null;
        private Exception resultException = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            WebServiceManager.ShootDown();
        }

        protected override void Because_of()
        {
            try
            {
                WebClient webClient = new WebClient();
                data = webClient.DownloadData(Request);
            }
            catch (Exception e)
            {
                resultException = e;
            }
        }

        [Test]
        public void Then_Web_exception_should_throwed_()
        {
            resultException.ShouldBeInstanceOfType<WebException>();
        }
    }


}
