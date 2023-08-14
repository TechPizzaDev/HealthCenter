using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
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
                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = @"call update_schedule(@doc_id, @hour, @days)";
                    cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));

                    foreach (ScheduleHour hour in _scheduleHours)
                    {
                        if (hour.Save())
                        {
                            cmd.Parameters.Add(new NpgsqlParameter("hour", hour.GetOffsetHour()));
                            cmd.Parameters.Add(new NpgsqlParameter("days", hour.GetDays()));

                            _ = await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
                ScheduleSaveButton.IsEnabled = false;
            });
        }
    }
}
