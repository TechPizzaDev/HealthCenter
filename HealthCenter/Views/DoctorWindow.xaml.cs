using System;
using System.Windows;
using Npgsql;

namespace HealthCenter.Views
{
    /// <summary>
    /// Interaction logic for DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        public NpgsqlConnection Connection { get; }
        public int EmployeeId { get; }

        public DoctorWindow(NpgsqlConnection connection, int employeeId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            EmployeeId = employeeId;

            InitializeComponent();
        }
    }
}
