using System;
using System.Windows;
using Npgsql;

namespace HealthCenter.Views
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public AdminWindow(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();
        }

        private void QueryToolButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow noticeWindow = new(Connection);
            noticeWindow.Show();
        }
    }
}
