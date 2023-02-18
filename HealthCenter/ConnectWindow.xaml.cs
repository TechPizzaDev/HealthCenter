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

            HostTextbox.Text = "pgserver.mau.se";
            PortTextbox.Text = 5432.ToString();
            DatabaseTextbox.Text = "an7044_db";

            UsernameTextbox.Text = "an7044";
            PasswordTextbox.Password = "nrhhseoc";
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

                Username = UsernameTextbox.Text,
                Password = PasswordTextbox.Password,

                ApplicationName = "Health Center"
            };
            return builder;
        }
    }
}
