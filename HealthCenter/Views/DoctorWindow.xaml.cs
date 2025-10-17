using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NodaTime;
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

        private List<ScheduleHour>? _scheduleHours;

        public DoctorWindow(NpgsqlConnection connection, int employeeId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            EmployeeId = employeeId;

            InitializeComponent();

            LoadSchedule();
        }

        public void LoadSchedule()
        {
            ScheduleSaveButton.IsEnabled = false;

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    _scheduleHours = await DbCalls.GetDoctorSchedule(Connection, EmployeeId, Item_Changed);
                    ScheduleGrid.ItemsSource = _scheduleHours;
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
            });
        }

        private bool Item_Changed(ScheduleHour obj, bool newValue)
        {
            ScheduleSaveButton.IsEnabled = true;
            return true;
        }

        private void ScheduleSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scheduleHours == null)
            {
                return;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    NpgsqlBatch batch = new(Connection);
                    string cmdText = @"call update_schedule(@doc_id, @hour, @days)";

                    foreach (ScheduleHour hour in _scheduleHours)
                    {
                        if (hour.Save())
                        {
                            NpgsqlBatchCommand cmd = new(cmdText);
                            cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));
                            cmd.Parameters.Add(new NpgsqlParameter("hour", hour.GetOffsetHour()));
                            cmd.Parameters.Add(new NpgsqlParameter("days", hour.GetDays()));
                            batch.BatchCommands.Add(cmd);
                        }
                    }

                    if (batch.BatchCommands.Count > 0)
                    {
                        _ = await batch.ExecuteNonQueryAsync();
                    }

                    ScheduleSaveButton.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
            });
        }

        private void AppointmentListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QueryWindow window = new(Connection);
                window.Title = "HealthCenter - Appointment List";
                window.Show();

                string cmdText =
                    "SELECT a.create_date, a.hour, a.day, " +
                    "p.medical_num, p.first_name, p.last_name, p.gender, p.phone, p.birth_date, p.address " +
                    "FROM appointments a " +
                    "INNER JOIN patients p " +
                    "ON (a.patient_id = p.id) " +
                    "WHERE (a.doc_id = @doc_id)";
                NpgsqlCommand cmd = new(cmdText, Connection);
                cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));
                window.TakeControl(cmd);
            }
            catch (Exception ex)
            {
                ex.ToMessageBox();
            }
        }

        private void CreateMedRecordButton_Click(object sender, RoutedEventArgs e)
        {
            CreateMedicalRecordWindow window = new(Connection, EmployeeId);
            window.Show();
        }

        private void MedRecordListButton_Click(object sender, RoutedEventArgs e)
        {
            QueryWindow window = new(Connection);
            window.Title = "HealthCenter - Medical Record List";
            window.Show();

            string cmdText =
                "SELECT p.medical_num, mr.diagnosis, mr.description, mr.prescription, mr.visit_date " +
                "FROM med_records mr " +
                "INNER JOIN patients p " +
                "ON p.id = mr.patient_id " +
                "WHERE mr.doc_id = @doc_id";
            NpgsqlCommand cmd = new(cmdText, Connection);
            cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));
            window.TakeControl(cmd);
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
                "  concat(p.first_name, ' ', p.last_name) as patient_name, p.phone as patient_phone " +
                "FROM notifications n " +
                "  JOIN patients p ON p.id = n.patient_id " +
                "  JOIN notify_appoint na ON na.notify_id = n.id " +
                "  JOIN appointments a ON a.id = na.appoint_id " +
                "WHERE n.employee_id = @employee_id";
            NpgsqlCommand cmd = new(cmdText, Connection);
            cmd.Parameters.Add(new NpgsqlParameter("employee_id", EmployeeId));

            window.TakeControl(cmd);
        }
    }
}
