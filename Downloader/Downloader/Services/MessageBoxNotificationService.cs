using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Downloader.Services
{
    /// <summary>
    /// Notify user by MessageBox window
    /// </summary>
    public class MessageBoxNotificationService : NotificationService
    {
        protected override void NotifyInform(string message, string header)
        {
            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void NotifyAlert(string message, string header)
        {
            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
