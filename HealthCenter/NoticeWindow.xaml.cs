using Npgsql;
using System.Windows;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for NoticeWindow.xaml
    /// </summary>
    public partial class NoticeWindow : Window
    {
        public NoticeWindow()
        {
            InitializeComponent();
        }

        public void AddNotice(object? sender, PostgresNotice notice)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.Items.Add(new NoticeItem(notice.Severity, notice.MessageText, notice.Where));
            });
        }

        public record NoticeItem(string Severity, string MessageText, string? Where);
    }
}
