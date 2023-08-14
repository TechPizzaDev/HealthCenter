using System;
using System.Windows;
using Npgsql;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for RegisterSpecializationWindow.xaml
    /// </summary>
    public partial class RegisterSpecializationWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public RegisterSpecializationWindow(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();

            NameTextbox.TextChanged += (s, e) => UpdateButton();
            CostTextbox.TextChanged += (s, e) => UpdateButton();
            UpdateButton();
        }

        private void UpdateButton()
        {
            bool enabled = true;
            enabled = enabled && NameTextbox.Text.Trim().Length > 0;
            enabled = enabled && CostTextbox.Text.Trim().Length > 0;
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
                    string name = NameTextbox.Text.Trim();
                    decimal cost = decimal.Parse(CostTextbox.Text.Trim());

                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = "CALL health_center.register_specialization(@name, @cost)";
                    cmd.Parameters.Add(new NpgsqlParameter("name", name));
                    cmd.Parameters.Add(new NpgsqlParameter("cost", cost));
                    await cmd.ExecuteNonQueryAsync();

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
