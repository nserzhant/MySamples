using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBehave.Spec.NUnit;
using Downloader.Services;
using Moq;
using System.IO;
using NUnit.Framework;

namespace Downloader.Tests.Integration.Services
{
    [Category("FileSystemManager")]
    public class When_working_with_file_system_manager : SpecBase
    {
        protected FileSystemManager FileSystemManager = null;
        protected Mock<ILogger> Logger = new Mock<ILogger>();

        protected override void Establish_context()
        {
            base.Establish_context();
            FileSystemManager = new FileSystemManager(Logger.Object);
        }
    }

    public class And_removing_file : When_working_with_file_system_manager
    {
        private readonly string filePath = "tempFile.bin";

        protected override void Establish_context()
        {
            base.Establish_context();
            this.createFile();
        }

        private void createFile()
        {
            deleteIfExists();
            File.WriteAllText(filePath, "someContent");
        }

        private void deleteIfExists()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        protected override void Because_of()
        {
            base.Because_of();
            FileSystemManager.RevmoveFile(filePath);
        }

        [Test]
        public void Then_file_should_be_deleted()
        {
            bool isExists = File.Exists(filePath);
            isExists.ShouldEqual(false);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            deleteIfExists();
        }
    }


    public class And_renaming_temp_file : When_working_with_file_system_manager
    {
        private readonly string filePath = "tempFile.bin";
        private readonly string tempFilePath = "tempFile.bin.tmp";

        protected override void Establish_context()
        {
            base.Establish_context();
            this.createTempFile();
        }

        private void createTempFile()
        {
            deleteIfExists(tempFilePath);
            deleteIfExists(filePath);

            File.WriteAllText(tempFilePath, "someContent");
        }

        private static void deleteIfExists(string pathToDelete)
        {
            if (File.Exists(pathToDelete))
            {
                File.Delete(pathToDelete);
            }
        }

        protected override void Because_of()
        {
            FileSystemManager.RenameTempFile(tempFilePath);
        }

        [Test]
        public void Then_only_renamed_file_should_be_exits()
        {
            bool isExists = File.Exists(filePath);
            bool isTempExits = File.Exists(tempFilePath);
            isExists.ShouldEqual(true);
            isTempExits.ShouldEqual(false);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            deleteIfExists(tempFilePath);
            deleteIfExists(filePath);
        }
    }
}
