using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using Downloader.Services;
using Moq;
using Downloader.Models;
using System.IO;
using System.Threading;
using NUnit.Framework;
using System.Security.Cryptography;
using WebService;

namespace Downloader.Tests.Integration.Services
{
    [Category("NetworkClient")]
    public class When_working_with_network_Client : SpecBase
    {
        protected NetworkClient NetworkClient = null;
        protected Mock<INetworkSettings> NetworkSettings = null;
        protected Mock<IUriSource> UriSource = null;
        protected SpeedMeasurer SpeedMeasurer = null;
        protected Mock<ILogger> Logger = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            NetworkSettings = new Mock<INetworkSettings>();
            UriSource = new Mock<IUriSource>();
            Logger = new Mock<ILogger>();
            SpeedMeasurer = new SpeedMeasurer();
            NetworkClient = new NetworkClient(NetworkSettings.Object, UriSource.Object,
                SpeedMeasurer, Logger.Object);
            NotificationService.Notifier = null;
        }
    }

    public class And_downloading_content_from_a_site : When_working_with_network_Client
    {
        private readonly string checkingFileName = "fileToCheck.bin";
        private readonly string contentDirectoryName = "content";
        private readonly long downloadContentSize = 1000000;
        private readonly string httpServiceEndpoint = @"http://localhost:4480/";

        private WebServiceManager webServiceManager = null;
        private string contentToDownloadPath = null;
        private MemoryStream saveDownloadedContentStream = null;
        private byte[] downladedContent = null;
        private bool downloadFailed = false;

        protected override void Establish_context()
        {
            base.Establish_context();
            contentToDownloadPath = Path.Combine(contentDirectoryName, checkingFileName);

            NetworkSettings.SetupGet(setting => setting.BlockSize).Returns(10000);
            NetworkSettings.SetupGet(setting => setting.ProxyIP).Returns("");
            NetworkSettings.SetupGet(setting => setting.ProxyPort).Returns(1043);
            NetworkSettings.SetupGet(settng => settng.UseProxy).Returns(false);

            string downloadUri = Path.Combine(httpServiceEndpoint, contentDirectoryName, checkingFileName).Replace("\\","/");

            UriSource.Setup(source => source.Uri).Returns(downloadUri);


            this.createDirectoryIfNotExists();
            this.createFileToDownloadInDirectory();
            webServiceManager = new WebServiceManager(contentDirectoryName, "");
            saveDownloadedContentStream = new MemoryStream();

            string errorMessage = "";
            if (!webServiceManager.TryStart(httpServiceEndpoint, out errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }
            NetworkClient.OnBlockDownloaded += new Action<byte[], int>(NetworkClient_OnBlockDownloaded);
        }

        private void NetworkClient_OnBlockDownloaded(byte[] contentBlock, int blockSize)
        {
            saveDownloadedContentStream.Write(contentBlock, 0, blockSize);
        }

        private void createFileToDownloadInDirectory()
        {
            deleteFileIfExists(contentToDownloadPath);
            createFile(contentToDownloadPath);
        }

        private void createFile(string pathToCreateFile)
        {
            using (var fileStream = File.Create(pathToCreateFile))
            {
                byte[] buffer = new byte[downloadContentSize];
                Random rand = new Random();
                rand.NextBytes(buffer);
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        private static void deleteFileIfExists(string fileToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }

        private void createDirectoryIfNotExists()
        {
            if (!Directory.Exists(contentDirectoryName))
            {
                Directory.CreateDirectory(contentDirectoryName);
            }
        }

        protected override void Because_of()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            NetworkClient.OnComplete += () => resetEvent.Set();
            NetworkClient.OnError += () =>
                {
                    downloadFailed = true;
                    resetEvent.Set();
                };

            NetworkClient.StartDownloading(0, downloadContentSize);
            resetEvent.WaitOne();
            saveDownloadedContentStream.Flush();
        }

        [Test]
        public void Then_downloaded_content_should_be_same_as_source_file()
        {
            byte[] downloadedContent = saveDownloadedContentStream.ToArray();
            byte[] sourceFileContent = File.ReadAllBytes(contentToDownloadPath);

            string downloadedHash = computeHash(downloadedContent);
            string sourceHash = computeHash(sourceFileContent);

            downloadedHash.ShouldEqual(sourceHash);
        }


        private string computeHash(byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var bytesHash = md5.ComputeHash(bytes);
            return Encoding.Default.GetString(bytesHash);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            webServiceManager.ShootDown();
            deleteFileIfExists(contentToDownloadPath);
            deleteFolderIfExsits(contentDirectoryName);
            this.saveDownloadedContentStream.Close();
            this.saveDownloadedContentStream.Dispose();
        }

        private void deleteFolderIfExsits(string contentDirectoryName)
        {
            if (Directory.Exists(contentDirectoryName))
            {
                Directory.Delete(contentDirectoryName);
            }
        }
    }
}
