using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            window.TakeControl(DbHelper.DoctorQuery("d.*, s.name specialization, s.cost"));
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

            var row = window.SelectedRow.Row;
            Dispatcher.InvokeAsync(async () => await CreateAppointment((int)row[0]));
        }

        private async Task CreateAppointment(int docId)
        {
            try
            {
                QueryWindow window = new(Connection);
                window.TakeControl();
                window.ConfirmButton.Visibility = Visibility.Visible;
                window.Title = "HealthCenter - Pick a Time";

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

                using NpgsqlCommand cmd = new();
                cmd.Connection = Connection;
                cmd.CommandText = "CALL health_center.create_appointment(@patient_id, @doc_id, @hour, @day)";
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
                    ((DataGrid)sender).UnselectAllCells();
                    break;
                }

                if (cell.Item is ScheduleHour hour)
                {
                    if (!hour.GetDays()[displayIndex - 1])
                    {
                        ((DataGrid)sender).UnselectAllCells();
                        break;
                    }
                }
            }
        }
    }
}
