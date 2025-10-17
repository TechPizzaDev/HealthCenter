using System;
using System.Windows;

namespace HealthCenter
{
    public static class ExceptionExtensions
    {
        public static void ToMessageBox(this Exception ex)
        {
#if DEBUG
            MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
#else
            MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }
    }
}
