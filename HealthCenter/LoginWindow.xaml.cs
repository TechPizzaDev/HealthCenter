using System;
using System.Threading.Tasks;
using System.Windows;
using HealthCenter.Views;
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
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as UIElement;
            if (element != null)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    MedicalNumber medNum = new(int.Parse(MedicalNumTextbox.Text.Replace(" ", "")));
                    byte[] password = DbCalls.MakePassword(PasswordTextbox.Password);

                    int userId = await DbCalls.Auth(Connection, medNum, password);
                    PatientWindow window = new(Connection, userId);
                    Application.Current.MainWindow = window;
                    window.Show();

                    Close();
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                    if (element != null)
                    {
                        element.IsEnabled = true;
                    }
                }
            });
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                RegisterWindow window = new(Connection);
                Application.Current.MainWindow = window;
                window.Show();

                Close();
            });
        }
    }
}
