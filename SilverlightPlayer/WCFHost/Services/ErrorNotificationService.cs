using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WCFHost.Services
{
    public interface IErrorNotificationService
    {
        void Notify(string message);
    }

    public class ErrorNotificationService : IErrorNotificationService
    {
        public void Notify(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
