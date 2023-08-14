using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        }

        public QueryWindow TakeControl(string query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Dispatch(query);
            InputQueryText.Text = query;
            TakeControl();
            return this;
        }

        public QueryWindow TakeControl()
        {
            InputQueryText.Visibility = Visibility.Collapsed;
            InputQueryLabel.Visibility = Visibility.Collapsed;

            UserGrid.SelectionChanged += UserGrid_SelectionChanged;
            UserGrid.SelectedCellsChanged += UserGrid_SelectedCellsChanged;
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
            Dispatcher.InvokeAsync(async () =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    StatusText.Text = $"Running query: {query}";
                    InputQueryText.IsEnabled = false;

                    int affectedRows = await RunQuery(query);
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
                }
            });
        }

        public async Task<int> RunQuery(string query)
        {
            using NpgsqlCommand cmd = new(query, Connection);
            using NpgsqlDataAdapter dataAdapter = new(cmd);

            DataSet dataSet = new();
            int affectedRows = await Task.Run(() => dataAdapter.Fill(dataSet));

            DataTable dataTable = dataSet.Tables[0];
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
