using System;
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

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    using NpgsqlCommand cmd = new();
                    cmd.Connection = connection;

                    cmd.CommandText = "INSERT INTO health_center.employees (employee_num, password) " +
                    "VALUES (@employee_num, @password) " +
                    "ON CONFLICT (employee_num) DO NOTHING;";
                    cmd.Parameters.Add(new NpgsqlParameter("employee_num", new EmployeeNumber("1")));
                    cmd.Parameters.Add(new NpgsqlParameter("password", DbHelper.MakePassword("admin")));
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
            });
        }

        private void LoginPatientButton_Click(object sender, RoutedEventArgs e)
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
                    MedicalNumber patientNum = new(UserNumTextbox.Text);
                    byte[] password = DbHelper.MakePassword(PasswordTextbox.Password);

                    int userId = await DbCalls.AuthPatient(Connection, patientNum, password);
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

        private void LoginEmployeeButton_Click(object sender, RoutedEventArgs e)
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
                    EmployeeNumber employeeNum = new(UserNumTextbox.Text);
                    byte[] password = DbHelper.MakePassword(PasswordTextbox.Password);

                    int employeeId = await DbCalls.AuthEmployee(Connection, employeeNum, password);

                    if (await DbCalls.IsAdmin(Connection, employeeId))
                    {
                        AdminWindow window = new(Connection, employeeId);
                        Application.Current.MainWindow = window;
                        window.Show();
                    }
                    else if (await DbCalls.IsDoctor(Connection, employeeId))
                    {
                        DoctorWindow window = new(Connection, employeeId);
                        Application.Current.MainWindow = window;
                        window.Show();
                    }
                    else
                    {
                        MessageBox.Show("Unassigned employee.");
                    }

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

            Dispatcher.InvokeAsync(() =>
            {
                RegisterPatientWindow window = new(Connection);
                Application.Current.MainWindow = window;
                window.Show();

                Close();
            });
        }
    }
}
