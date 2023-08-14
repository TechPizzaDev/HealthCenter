using System;
using System.Windows;
using HealthCenter.Views;
using Npgsql;
using NpgsqlTypes;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for RegisterDoctorWindow.xaml
    /// </summary>
    public partial class RegisterDoctorWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public RegisterDoctorWindow(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();

            EmployeeNumTextbox.TextChanged += (s, e) => UpdateButton();
            FullNameTextbox.TextChanged += (s, e) => UpdateButton();
            PhoneNumberTextbox.TextChanged += (s, e) => UpdateButton();
            PasswordTextbox.PasswordChanged += (s, e) => UpdateButton();
            UpdateButton();
        }

        private void UpdateButton()
        {
            bool enabled = true;
            enabled = enabled && EmployeeNumTextbox.Text.Replace(" ", "").Length > 0;
            enabled = enabled && PasswordTextbox.Password.Length > 3;
            enabled = enabled && FullNameTextbox.Text.Trim().Length > 0;
            enabled = enabled && PhoneNumberTextbox.Text.Replace(" ", "").Length > 0;
            RegisterButton.IsEnabled = enabled;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    EmployeeNumber employeeNum = new(int.Parse(EmployeeNumTextbox.Text.Replace(" ", "")));
                    string fullName = FullNameTextbox.Text.Trim();
                    string phone = PhoneNumberTextbox.Text.Trim();
                    byte[] password = DbHelper.MakePassword(PasswordTextbox.Password);

                    int employeeId = await DbCalls.RegisterDoctor(Connection, employeeNum, fullName, phone, password);

                    Close();
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                    UpdateButton();
                }
            });
        }
    }
}
