using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

            Dispatcher.InvokeAsync(async () =>
            {
                using NpgsqlCommand cmd = new();
                cmd.Connection = Connection;

                //cmd.CommandText = $"INSERT INTO table_name (column1, column2, column3,..) VALUES ( value1, value2, value3,..);";

                for (int i = 0; i < 10; i++)
                {
                    cmd.CommandText = @"call health_center.register_user('patient', 'Bert', 'Il', '\x1234567890'::bytea, '\x1234567890'::bytea, 'pro@google.com')";
                    await cmd.ExecuteNonQueryAsync();

                    await Task.Delay(500);
                }
            });
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {

            });
        }
    }
}
