using System;
using System.Data;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using HealthCenter.Views;
using Npgsql;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for QueryWindow.xaml
    /// </summary>
    public partial class QueryWindow : Window
    {
        public NpgsqlConnection Connection { get; }

        public DataRowView? SelectedRow { get; private set; }
        public DataGridCellInfo? SelectedCell { get; private set; }

        public QueryWindow(NpgsqlConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();

            UserGrid.IsReadOnly = true;
            ConfirmButton.Visibility = Visibility.Hidden;

            UserGrid.SelectionChanged += UserGrid_SelectionChanged;
            UserGrid.SelectedCellsChanged += UserGrid_SelectedCellsChanged;
            UserGrid.AutoGeneratingColumn += UserGrid_AutoGeneratingColumn;
        }

        public QueryWindow TakeControl(string query)
        {
            NpgsqlCommand cmd = new(query, Connection);
            return TakeControl(cmd);
        }

        public QueryWindow TakeControl(NpgsqlCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            Dispatch(command);
            InputQueryText.Text = command.CommandText;

            TakeControl();
            return this;
        }

        public QueryWindow TakeControl()
        {
            InputQueryText.Visibility = Visibility.Collapsed;
            InputQueryLabel.Visibility = Visibility.Collapsed;
            return this;
        }

        private void UserGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is DataRowView row)
            {
                SelectedRow = row;
            }
            else
            {
                SelectedRow = null;
            }
            ConfirmButton.IsEnabled = SelectedRow != null;
        }

        private void UserGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count > 0 && e.AddedCells[0] is DataGridCellInfo cell)
            {
                SelectedCell = cell;
            }
            else
            {
                SelectedCell = null;
            }
            ConfirmButton.IsEnabled = SelectedCell.HasValue;
        }

        private void UserGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn column)
            {
                column.Binding = new Binding()
                {
                    Path = ((Binding) column.Binding).Path,
                    Converter = new ScheduleConverter(),
                };
            }
        }

        private void InputQueryText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }

            Dispatch(InputQueryText.Text);
        }

        private void Dispatch(string query)
        {
            NpgsqlCommand cmd = new(query, Connection);
            Dispatch(cmd);
        }

        private void Dispatch(NpgsqlCommand command)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    StatusText.Text = $"Running command: {command}";
                    InputQueryText.IsEnabled = false;

                    int affectedRows = await RunAndFillUserGrid(command);
                    StatusText.Text = $"Query finished in {stopwatch.ElapsedMilliseconds}ms: Affected {affectedRows} rows";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Query threw {ex.GetType().Name} in {stopwatch.ElapsedMilliseconds}ms: {ex.Message.ReplaceLineEndings(" ")}";
                }
                finally
                {
                    InputQueryText.IsEnabled = true;
                    InputQueryText.Focus();

                    await command.DisposeAsync();
                }
            });
        }

        public async Task<int> RunAndFillUserGrid(NpgsqlCommand command)
        {
            using NpgsqlDataAdapter dataAdapter = new(command);

            DataTable dataTable = new();
            int affectedRows = await DbCalls.FillAsync(dataAdapter, dataTable, true, default);

            UserGrid.ItemsSource = dataTable.DefaultView;

            return affectedRows;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
