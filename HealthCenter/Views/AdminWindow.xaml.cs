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

        private void QueryToolButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.TakeControl("SELECT * FROM patients");
            window.Show();
        }

        private void RegisterDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterDoctorWindow window = new(Connection);
            window.Show();
        }

        private void DoctorListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.TakeControl(DbHelper.DoctorQuery("d.*, s.name specialization"));
            window.Show();
        }

        private void RegisterSpecializationButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterSpecializationWindow window = new(Connection);
            window.Show();
        }

        private void SpecializationListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.TakeControl("SELECT * FROM specializations");
            window.Show();
        }

        private void PatientListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.TakeControl("SELECT * FROM patients");
            window.Show();
        }
    }
}
