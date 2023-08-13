using System;
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

            if (app.PasswordArg != null)
            {
                ConnectButton_Click(this, new RoutedEventArgs());
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    NpgsqlDataSourceBuilder dataSourceBuilder = new(GetConnectionStringBuilder().ToString());
                    dataSourceBuilder.MapComposite<MedicalNumber>("health_center.medical_num");
                    await using var dataSource = dataSourceBuilder.Build();

                    NpgsqlConnection conn = await dataSource.OpenConnectionAsync();

#if DEBUG
                    NoticeWindow noticeWindow = new();
                    conn.Notice += (sender, ev) => noticeWindow.AddNotice(sender, ev.Notice);
                    noticeWindow.Show();
#endif

                    LoginWindow window = new(conn);
                    Application.Current.MainWindow = window;
                    window.Show();

                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
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

                IncludeErrorDetail = true,

                ApplicationName = "Health Center"
            };
            return builder;
        }
    }
}
