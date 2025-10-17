using System;
using System.Data;
using System.Threading.Tasks;
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

        [Obsolete]
        private void QueryToolButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Patient List";
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
            window.Title = "Health Center - Doctor List";
            window.TakeControl("SELECT * FROM patient_appointment_choices");
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
            window.Title = "Health Center - Specialization List";
            window.TakeControl("SELECT * FROM specializations");
            window.Show();
        }

        private void PatientListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Patient List";
            window.TakeControl("SELECT * FROM admin_patient_list");
            window.Show();
        }

        private void MedRecordListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Medical Record List";
            window.TakeControl("SELECT * FROM med_records");
            window.Show();
        }

        private void PatientSpendingButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Patient Spending";
            window.TakeControl("SELECT * FROM admin_patient_spending");
            window.Show();
        }

        private void AppointmentListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Appointment List";
            window.TakeControl("SELECT * FROM get_all_appointments()");
            window.Show();
        }

        private void DeleteDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.ConfirmButton.Visibility = Visibility.Visible;
            window.Title = "HealthCenter - Pick an Employee to Delete";
            window.TakeControl("SELECT * FROM patient_appointment_choices");
            if (!window.ShowDialog().GetValueOrDefault())
            {
                MessageBox.Show("Deletion cancelled.");
                return;
            }

            if (window.SelectedRow == null)
            {
                MessageBox.Show("No employee selected.");
                return;
            }

            DataRow row = window.SelectedRow.Row;
            int employeeId = row.Field<int>("doc_id");
            Dispatcher.InvokeAsync(async () =>
            {
                if (await DeleteEmployee(employeeId))
                {
                    MessageBox.Show($"Deleted employee with ID {employeeId}.", "Error");
                }
                else
                {
                    MessageBox.Show($"Failed to delete employee with ID {employeeId}.", "Success");
                }
            });
        }

        private async Task<bool> DeleteEmployee(int employeeId)
        {
            try
            {
                string cmdText = "CALL health_center.delete_employee(@employee_id)";
                using NpgsqlCommand cmd = new(cmdText, Connection);
                cmd.Parameters.Add(new NpgsqlParameter("employee_id", employeeId));
                return await cmd.ExecuteNonQueryAsync() != 0;
            }
            catch (Exception ex)
            {
                ex.ToMessageBox();
                return false;
            }
        }
    }
}
