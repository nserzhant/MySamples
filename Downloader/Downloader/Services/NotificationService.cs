using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Downloader.Services
{
    /// <summary>
    /// Notify users about something )
    /// </summary>
    public abstract class NotificationService
    {
        protected abstract void NotifyInform(string message, string header);
        protected abstract void NotifyAlert(string message, string header);

        private static NotificationService notifier = new MessageBoxNotificationService();


        public static void Inform(string message, string header)
        {
            if (notifier != null)
            {
                notifier.NotifyInform(message, header);
            }
        }

        public static NotificationService Notifier
        {
            get { return notifier; }
            set { notifier = value; }
        }

        public static void Alert(string message, string header)
        {
            if (notifier != null)
            {
                notifier.NotifyAlert(message, header);
            }
        }


    }
}
