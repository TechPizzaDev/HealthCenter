using System;
using System.Collections;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodaTime;
using Npgsql;

namespace HealthCenter.Views
{
    /// <summary>
    /// Interaction logic for PatientWindow.xaml
    /// </summary>
    public partial class PatientWindow : Window
    {
        public NpgsqlConnection Connection { get; }
        public int PatientId { get; }

        public PatientWindow(NpgsqlConnection connection, int patientId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            PatientId = patientId;

            InitializeComponent();
        }

        private void CreateAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.TakeControl("SELECT * FROM health_center.patient_appointment_choices");
            window.ConfirmButton.Visibility = Visibility.Visible;
            window.Title = "HealthCenter - Pick a Doctor";

            if (!window.ShowDialog().GetValueOrDefault())
            {
                MessageBox.Show("Appointment cancelled.");
                return;
            }
            if (window.SelectedRow == null)
            {
                MessageBox.Show("No doctor selected.");
                return;
            }

            DataRow row = window.SelectedRow.Row;
            int docId = row.Field<int>("doc_id");
            Dispatcher.InvokeAsync(async () => await CreateAppointment(docId));
        }

        private void AppointmentListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QueryWindow window = new(Connection);
                window.TakeControl();
                window.Title = "Health Center - Appointment List";
                window.Show();

                NpgsqlCommand cmd = new("SELECT * FROM get_appointment_list(@patient_id)", Connection);
                cmd.Parameters.Add(new NpgsqlParameter("patient_id", PatientId));
                window.TakeControl(cmd);
            }
            catch (Exception ex)
            {
                ex.ToMessageBox();
            }
        }

        private async Task CreateAppointment(int docId)
        {
            try
            {
                QueryWindow window = new(Connection);
                window.TakeControl();
                window.ConfirmButton.Visibility = Visibility.Visible;
                window.Title = "Health Center - Pick a Time";

                window.UserGrid.SelectionUnit = DataGridSelectionUnit.Cell;
                window.UserGrid.SelectionMode = DataGridSelectionMode.Single;
                window.UserGrid.CanUserReorderColumns = false;
                window.UserGrid.SelectedCellsChanged += UserGrid_SelectedCellsChanged;
                window.UserGrid.ItemsSource = await DbCalls.GetDoctorSchedule(Connection, docId, null);

                if (!window.ShowDialog().GetValueOrDefault())
                {
                    MessageBox.Show("Appointment cancelled.");
                    return;
                }

                if (!window.SelectedCell.HasValue)
                {
                    MessageBox.Show("No time selected.");
                    return;
                }

                var selectedCell = window.SelectedCell.Value;
                if (selectedCell.Item is not ScheduleHour hour)
                {
                    MessageBox.Show("Unknown time.");
                    return;
                }

                BitArray day = new(5);
                day.Set(selectedCell.Column.DisplayIndex - 1, true);

                string cmdText = "SELECT health_center.create_appointment(@patient_id, @doc_id, @hour, @day)";
                using NpgsqlCommand cmd = new(cmdText, Connection);
                cmd.Parameters.Add(new NpgsqlParameter("patient_id", PatientId));
                cmd.Parameters.Add(new NpgsqlParameter("doc_id", docId));
                cmd.Parameters.Add(new NpgsqlParameter("hour", hour.GetOffsetHour()));
                cmd.Parameters.Add(new NpgsqlParameter("day", day));
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ex.ToMessageBox();
            }
        }

        private void UserGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var cell in e.AddedCells)
            {
                int displayIndex = cell.Column.DisplayIndex;
                if (displayIndex == 0)
                {
                    ((DataGrid) sender).UnselectAllCells();
                    break;
                }

                if (cell.Item is ScheduleHour hour)
                {
                    if (!hour.GetDays()[displayIndex - 1])
                    {
                        ((DataGrid) sender).UnselectAllCells();
                        break;
                    }
                }
            }
        }

        private void MedRecordListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Medical Record List";
            window.Show();

            string cmdText =
                "SELECT mr.diagnosis, mr.description, mr.prescription, d.full_name as doc_name, d.phone as doc_phone " +
                "FROM med_records mr " +
                "JOIN doctors d ON d.employee_id = mr.doc_id " +
                "WHERE mr.patient_id = @patient_id";
            NpgsqlCommand cmd = new(cmdText, Connection);
            cmd.Parameters.Add(new NpgsqlParameter("patient_id", PatientId));

            window.TakeControl(cmd);
        }

        private void EditPersonalButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    RegisterPatientWindow window = new(Connection);
                    window.Title = "Health Center - Edit Personal Info";
                    window.SetInputsEnabled(false);
                    window.MedicalNumTextbox.IsEnabled = false;
                    window.RegisterButton.Content = "Apply";
                    window.Show();

                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = "SELECT * FROM patients WHERE id = @patient_id";
                    cmd.Parameters.Add(new NpgsqlParameter("patient_id", PatientId));

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        window.MedicalNumTextbox.Text = reader.GetFieldValue<MedicalNumber>("medical_num").ToString();
                        window.FirstNameTextbox.Text = reader.GetString("first_name");
                        window.LastNameTextbox.Text = reader.GetString("last_name");
                        window.GenderTextbox.Text = reader.GetString("gender");
                        window.AddressTextbox.Text = reader.GetString("address");
                        window.PhoneNumberTextbox.Text = reader.GetString("phone");
                        window.BirthDatePicker.SelectedDate = reader.GetFieldValue<LocalDate>("birth_date").ToDateTimeUnspecified();

                        window.SetInputsEnabled(true);
                    }
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
            });
        }

        private void NotificationListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "Health Center - Notification List";
            window.Show();

            string cmdText =
                "SELECT " +
                "  n.message, n.create_date, " +
                "  a.hour as appoint_hour, a.day as appoint_day, " +
                "  d.full_name as doc_name, d.phone as doc_phone " +
                "FROM notifications n " +
                "  JOIN doctors d ON d.employee_id = n.employee_id " +
                "  JOIN notify_appoint na ON na.notify_id = n.id " +
                "  JOIN appointments a ON a.id = na.appoint_id " +
                "WHERE n.patient_id = @patient_id";
            NpgsqlCommand cmd = new(cmdText, Connection);
            cmd.Parameters.Add(new NpgsqlParameter("patient_id", PatientId));

            window.TakeControl(cmd);
        }
    }
}
