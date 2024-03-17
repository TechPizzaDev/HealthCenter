using System;
using System.Security.Authentication;
using System.Windows;
using HealthCenter.Views;
using NodaTime;
using Npgsql;
using NpgsqlTypes;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for RegisterPatientWindow.xaml
    /// </summary>
    public partial class RegisterPatientWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public RegisterPatientWindow(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();

            MedicalNumTextbox.TextChanged += (s, e) => UpdateButton();
            FirstNameTextbox.TextChanged += (s, e) => UpdateButton();
            LastNameTextbox.TextChanged += (s, e) => UpdateButton();
            GenderTextbox.TextChanged += (s, e) => UpdateButton();
            AddressTextbox.TextChanged += (s, e) => UpdateButton();
            PhoneNumberTextbox.TextChanged += (s, e) => UpdateButton();
            BirthDatePicker.SelectedDateChanged += (s, e) => UpdateButton();
            PasswordTextbox.PasswordChanged += (s, e) => UpdateButton();
            UpdateButton();
        }

        private void UpdateButton()
        {
            bool enabled = true;
            enabled = enabled && MedicalNumTextbox.Text.Replace(" ", "").Length == 9;
            enabled = enabled && PasswordTextbox.Password.Length > 3;
            enabled = enabled && FirstNameTextbox.Text.Trim().Length > 0;
            enabled = enabled && LastNameTextbox.Text.Trim().Length > 0;
            enabled = enabled && GenderTextbox.Text.Trim().Length > 0;
            enabled = enabled && BirthDatePicker.SelectedDate.HasValue;
            enabled = enabled && AddressTextbox.Text.Trim().Length > 0;
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
                    MedicalNumber medNum = new(int.Parse(MedicalNumTextbox.Text.Replace(" ", "")));
                    byte[] password = DbHelper.MakePassword(PasswordTextbox.Password);
                    DateTime birthDate = BirthDatePicker.SelectedDate!.Value;

                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = @"select register_patient(@med_num, @first_name, @last_name, @gender, @address, @phone, @birth_date, @password)";
                    cmd.Parameters.Add(new NpgsqlParameter("med_num", medNum));
                    cmd.Parameters.Add(new NpgsqlParameter("first_name", FirstNameTextbox.Text.Trim()) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("last_name", LastNameTextbox.Text.Trim()) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("gender", GenderTextbox.Text.Trim()) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("address", AddressTextbox.Text.Trim()) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("phone", PhoneNumberTextbox.Text.Replace(" ", "")) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("birth_date", LocalDate.FromDateTime(birthDate)));
                    cmd.Parameters.Add(new NpgsqlParameter("password", password));

                    object? result = await cmd.ExecuteScalarAsync();
                    if (!DbCalls.TryConvertMedicalNum(result, out int userId))
                    {
                        throw new InvalidCredentialException("The given Patient credentials are not valid.");
                    }

                    PatientWindow window = new(Connection, userId);
                    Application.Current.MainWindow = window;
                    window.Show();

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
