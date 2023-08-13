using System;
using System.Windows;
using Npgsql;

namespace HealthCenter.Views
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public NpgsqlConnection Connection { get; }
        public int EmployeeId { get; }

        public AdminWindow(NpgsqlConnection connection, int employeeId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            EmployeeId = employeeId;

            InitializeComponent();
        }

        private void RegisterDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterDoctorWindow window = new(Connection);
            window.Show();
        }

        private void QueryToolButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Show();
        }
    }
}
