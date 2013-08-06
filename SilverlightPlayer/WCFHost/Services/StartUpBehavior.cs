using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace WCFHost.Services
{
    public interface IStartUpBehavior
    {
        void OnStart(string uri);
    }


    public class StartUpBehavior : IStartUpBehavior
    {
        public void OnStart(string uri)
        {
            checkVideosFolder();
            startBrowser(uri);
        }

        private void startBrowser(string uri)
        {
            string fileToDisplay = @"content/silverlight/SilverlightPlayer.html"; //@"content/start.html";
            string openPath = string.Concat(uri, fileToDisplay);
            Process.Start(openPath);
        }

        private void checkVideosFolder()
        {
            string checkingFolderPath = @"content/videos";
            if (!Directory.Exists(checkingFolderPath))
            {
                Directory.CreateDirectory(checkingFolderPath);
            }
        }
    }
}
