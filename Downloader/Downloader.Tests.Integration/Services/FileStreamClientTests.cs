using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using Downloader.Services;
using Moq;
using Downloader.Models;
using System.IO;
using NUnit.Framework;
using System.Security.Cryptography;

namespace Downloader.Tests.Integration.Services
{
    [Category("FileStreamClient")]
    public class When_working_with_fileStream_client : SpecBase
    {
        private readonly string fileName = "testingFileStreamClient.bin";
        private readonly string folderPath = "";

        protected Mock<IFilePathSource> FilePathSource = null;
        protected string FilePath = null;

        protected override void Establish_context()
        {
            base.Establish_context();
            FilePath = Path.Combine(folderPath, fileName);
            FilePathSource = new Mock<IFilePathSource>();
            FilePathSource.SetupGet(source => source.SaveFolderPath).Returns(folderPath);
            FilePathSource.SetupGet(source => source.FileName).Returns(fileName);

            deleteFileIfExists();
        }

        private void deleteFileIfExists()
        {

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            deleteFileIfExists();
        }
    }

    public class And_reading_and_writing_data_by_filestream_clients : When_working_with_fileStream_client
    {
        private readonly long offsetPerStep = 500;
        private readonly int streamsCount = 2;
        private readonly int stepsCount = 10;

        private MemoryStream copyToStream = null;
        private Random rand = new Random(1);
        private byte[] writtenData = null;

        private List<FileStreamClient> fileStreamsClients = new List<FileStreamClient>();

        protected override void Establish_context()
        {
            base.Establish_context();

            for (int i = 0; i < streamsCount; i++)
            {
                FileStreamClient newClient = new FileStreamClient(FilePathSource.Object);
                fileStreamsClients.Add(newClient);
            }
            copyToStream = new MemoryStream();
        }

        protected override void Because_of()
        {
            for (int i = 0; i < fileStreamsClients.Count; i++)
            {
                fileStreamsClients[i].Open(i * offsetPerStep * stepsCount);
            }

            for (int step = 0; step < stepsCount; step++)
            {
                for (int i = 0; i < fileStreamsClients.Count; i++)
                {
                    byte[] dataToWrite = createDataToWrite(i, step);
                    fileStreamsClients[i].Write(dataToWrite, dataToWrite.Length);
                }
            }

            writtenData = copyToStream.ToArray();
            closeStreams();
        }

        private byte[] createDataToWrite(int fileStreamClientIndex, int step)
        {
            long offset = (fileStreamClientIndex * stepsCount + step) * offsetPerStep;
            copyToStream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = generateBytes(offsetPerStep);
            copyToStream.Write(buffer, 0, buffer.Length);
            return buffer;
        }

        private byte[] generateBytes(long size)
        {
            byte[] buffer = new byte[size];
            rand.NextBytes(buffer);
            return buffer;
        }

        [Test]
        public void Then_written_to_file_data_have_to_be_valid()
        {
            byte[] fromFileData = File.ReadAllBytes(FilePath);

            string hashWritten = computeHash(writtenData);
            string hashFromFile = computeHash(fromFileData);

            hashWritten.ShouldEqual(hashFromFile);
        }

        public string computeHash(byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var bytesHash = md5.ComputeHash(bytes);
            return Encoding.Default.GetString(bytesHash);
        }

        protected override void Cleanup()
        {
            closeStreams();
            base.Cleanup();
        }

        private void closeStreams()
        {
            foreach (var fileStreamClient in fileStreamsClients)
            {
                fileStreamClient.Close();
            }
        }
    }
}
