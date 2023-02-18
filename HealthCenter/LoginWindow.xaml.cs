using System.Text.RegularExpressions;
using System.Windows;
using Npgsql;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public LoginWindow(NpgsqlConnection connection)
        {
            Connection = connection;

            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                using NpgsqlCommand cmd = new();
                cmd.Connection = Connection;

                cmd.CommandText = $"DROP TABLE IF EXISTS teachers";
                await cmd.ExecuteNonQueryAsync();
                
                cmd.CommandText = "CREATE TABLE teachers (id SERIAL PRIMARY KEY," +
                    "first_name VARCHAR(255)," +
                    "last_name VARCHAR(255)," +
                    "subject VARCHAR(255)," +
                    "salary INT)";
                await cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
