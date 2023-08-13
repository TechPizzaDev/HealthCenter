using System;
using System.Windows;
using Npgsql;

namespace HealthCenter.Views
{
    /// <summary>
    /// Interaction logic for PatientWindow.xaml
    /// </summary>
    public partial class PatientWindow : Window
    {
        public NpgsqlConnection Connection { get; }
        public int UserId { get; }

        public PatientWindow(NpgsqlConnection connection, int userId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            UserId = userId;

            InitializeComponent();
        }
    }
}
