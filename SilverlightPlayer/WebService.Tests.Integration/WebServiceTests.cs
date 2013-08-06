using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using System.ServiceModel.Channels;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Moq;
using System.ServiceModel.Web;

namespace WebService.Tests.Integration
{
    [Category("WebService")]
    public class When_working_with_WebService : SpecBase
    {
        protected readonly string CheckingFileName = "fileToCheck.bin";
        protected readonly string ContentDirectoryName = "content";
        private readonly int contentFileSize = 10000;

        protected WebService WebService = null;
        protected string FilePath = "";
        private MockedWebOperationContext mockedContext = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            FilePath = Path.Combine(ContentDirectoryName, CheckingFileName);
            WebService = new WebService(string.Empty, ContentDirectoryName);
            createTempFolder();

            Mock<IWebOperationContext> mockContext =
                new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            mockedContext = new MockedWebOperationContext(mockContext.Object);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            dropTemFolder();
            mockedContext.Dispose();
        }

        private void createTempFolder()
        {
            if (!Directory.Exists(ContentDirectoryName))
            {
                Directory.CreateDirectory(ContentDirectoryName);
            }
        }

        private void dropTemFolder()
        {
            if (Directory.Exists(ContentDirectoryName))
            {
                Directory.Delete(ContentDirectoryName, true);
            }
        }

        protected void createTempFile()
        {
            string filePath = Path.Combine(ContentDirectoryName, CheckingFileName);

            using (var fileStream = File.Create(filePath))
            {
                byte[] buffer = new byte[contentFileSize];
                Random rand = new Random();
                rand.NextBytes(buffer);
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Flush();
                fileStream.Close();
            }
        }


        protected string ComputeHash(byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var bytesHash = md5.ComputeHash(bytes);
            return Encoding.Default.GetString(bytesHash);
        }
    }

    public class And_getting_content : When_working_with_WebService
    {
        private Stream readingStream = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            createTempFile();
        }

        protected override void Because_of()
        {
            readingStream = WebService.GetFile(FilePath);
        }

        [Test]
        public void Then_content_of_the_file_from_content_folder_should_be_returned()
        {
            byte[] downloadedContent = getDownloadContent();
            byte[] sourceContent = File.ReadAllBytes(FilePath);

            string downloadContentHash = ComputeHash(downloadedContent);
            string sourceContentHash = ComputeHash(sourceContent);

            downloadContentHash.ShouldEqual(sourceContentHash);
        }


        private byte[] getDownloadContent()
        {
            byte[] downloadedContent = null;
            using (MemoryStream memStr = new MemoryStream())
            {
                using (Stream stream = readingStream)
                {
                    int count = 0;
                    byte[] buffer = new byte[10000];
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memStr.Write(buffer, 0, count);
                    }

                    stream.Close();
                }

                memStr.Flush();
                downloadedContent = memStr.ToArray();
                memStr.Close();
            }
            return downloadedContent;
        }
    }

    public class And_getting_content_list : When_working_with_WebService
    {
        private Stream readingStream = null;
        private string expected = "";

        protected override void Establish_context()
        {
            base.Establish_context();
            createTempFile();
            expected = String.Format(@"<table><tr><td><div class=""fileInfo""><a class=""target_silverlight"" href=""#{0}"">{1}</a><div/><td/></tr></table>",
                          FilePath.Replace(@"\", @"/"),
                          CheckingFileName);

        }

        protected override void Because_of()
        {
            readingStream = WebService.GetContentList();
        }

        [Test]
        public void Then_a_list_of_the_files_should_be_returned()
        {
            string content = getOutGoingContent();
            expected.ShouldEqual(content);
        }

        private string getOutGoingContent()
        {
            string result = "";
            using (readingStream)
            {
                using (StreamReader reader = new StreamReader(readingStream))
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }
                readingStream.Close();
            }
            return result;
        }
    }

    public class And_upploading_file : When_working_with_WebService
    {
        private readonly int uploadingContentFileSize = 10000;
        private byte[] uploadingContent = null;
        private byte[] header = null;
        private MemoryStream memStr = new MemoryStream();



        protected override void Establish_context()
        {
            base.Establish_context();
            uploadingContent = getTempContent();

            string fileHeader = string.Concat(@"-----------------------------26777155735097
Content-Disposition: form-data; name=""FName""; filename=""",CheckingFileName,@"""
Content-Type: application/x-javascript","\r\n\r\n");
            header = Encoding.Default.GetBytes(fileHeader);

            memStr.Write(header, 0, header.Length);
            memStr.Write(uploadingContent, 0, uploadingContent.Length);
            memStr.Seek(0, SeekOrigin.Begin);
        }

        private byte[] getTempContent()
        {
            byte[] content = new byte[uploadingContentFileSize];
            Random rand = new Random();
            rand.NextBytes(content);
            return content;
        }

        protected override void Because_of()
        {
            WebService.UploadFile(memStr);
        }

        [Test]
        public void Then_content_with_expected_file_name_should_be_uploaded()
        {
            byte[] uploadedContent = getContentFromFile();

            string uploadingContentHash = ComputeHash(uploadingContent);
            string uploadedContentHash = ComputeHash(uploadedContent);

            uploadedContent.ShouldEqual(uploadedContent);
        }

        private byte[] getContentFromFile()
        {
            return File.ReadAllBytes(FilePath);
        }
    }
}
