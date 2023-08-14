using System;
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
                    _scheduleHours = new List<ScheduleHour>();
                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = "SELECT * FROM doc_schedule WHERE doc_id = @doc_id;";
                    cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));

                    await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync(default))
                    {
                        var hour = reader.GetFieldValue<OffsetTime>(1);
                        bool day1 = reader.GetBoolean(2);
                        bool day2 = reader.GetBoolean(3);
                        bool day3 = reader.GetBoolean(4);
                        bool day4 = reader.GetBoolean(5);
                        bool day5 = reader.GetBoolean(6);

                        ScheduleHour item = new(hour, day1, day2, day3, day4, day5);
                        item.Changed += Item_Changed;
                        _scheduleHours.Add(item);
                    }
                    ScheduleGrid.ItemsSource = _scheduleHours;
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                }
            });
        }

        private void Item_Changed(ScheduleHour obj)
        {
            ScheduleSaveButton.IsEnabled = true;
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
                    cmd.CommandText = @"call update_schedule(@doc_id, @hour, @day1, @day2, @day3, @day4, @day5)";
                    cmd.Parameters.Add(new NpgsqlParameter("doc_id", EmployeeId));

                    foreach (ScheduleHour hour in _scheduleHours)
                    {
                        if (hour.Save())
                        {
                            cmd.Parameters.Add(new NpgsqlParameter("hour", hour.GetOffsetHour()));
                            cmd.Parameters.Add(new NpgsqlParameter("day1", hour.Monday));
                            cmd.Parameters.Add(new NpgsqlParameter("day2", hour.Tuesday));
                            cmd.Parameters.Add(new NpgsqlParameter("day3", hour.Wednesday));
                            cmd.Parameters.Add(new NpgsqlParameter("day4", hour.Thursday));
                            cmd.Parameters.Add(new NpgsqlParameter("day5", hour.Friday));

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
