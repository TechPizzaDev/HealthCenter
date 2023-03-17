using System.Windows;
using Npgsql;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();

            App app = (App)Application.Current;
            HostTextbox.Text = app.HostArg;
            PortTextbox.Text = app.PortArg;
            DatabaseTextbox.Text = app.DatabaseArg;
            SchemaTextbox.Text = app.SchemaArg;
            UsernameTextbox.Text = app.UsernameArg;
            PasswordTextbox.Password = app.PasswordArg;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                NpgsqlConnection conn = new(GetConnectionStringBuilder().ToString());

                NoticeWindow noticeWindow = new();
                conn.Notice += (sender, ev) => noticeWindow.AddNotice(sender, ev.Notice);
                noticeWindow.Show();

                await conn.OpenAsync();

                LoginWindow window = new(conn);
                Application.Current.MainWindow = window;
                window.Show();

                Close();
            });
        }

        public NpgsqlConnectionStringBuilder GetConnectionStringBuilder()
        {
            NpgsqlConnectionStringBuilder builder = new()
            {
                Host = HostTextbox.Text,
                Port = int.Parse(PortTextbox.Text),
                Database = DatabaseTextbox.Text, 
                SearchPath = SchemaTextbox.Text,

                Username = UsernameTextbox.Text,
                Password = PasswordTextbox.Password,

                ApplicationName = "Health Center"
            };
            return builder;
        }
    }
}
