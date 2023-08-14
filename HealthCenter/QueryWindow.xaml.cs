using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
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

        public QueryWindow(NpgsqlConnection connection, string? query)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            InitializeComponent();

            if (query != null)
            {
                Dispatch(query);

                InputQueryText.Text = query;
                InputQueryText.Visibility = Visibility.Collapsed;
                InputQueryLabel.Visibility = Visibility.Collapsed;
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
    }
}
