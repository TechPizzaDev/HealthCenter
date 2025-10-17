using System;
using System.Windows;
using Npgsql;
using NpgsqlTypes;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for CreateMedicalRecordWindow.xaml
    /// </summary>
    public partial class CreateMedicalRecordWindow : Window
    {
        public NpgsqlConnection Connection { get; }
        public int DoctorId { get; }

        public CreateMedicalRecordWindow(NpgsqlConnection connection, int doctorId)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            DoctorId = doctorId;

            InitializeComponent();

            PatientNumTextbox.TextChanged += (s, e) => UpdateButton();
            DiagnosisTextbox.TextChanged += (s, e) => UpdateButton();
            PrescriptionTextbox.TextChanged += (s, e) => UpdateButton();
            DescriptionTextbox.TextChanged += (s, e) => UpdateButton();
            UpdateButton();
        }

        private void UpdateButton()
        {
            bool enabled = true;
            enabled = enabled && PatientNumTextbox.Text.Replace(" ", "").Length > 0;
            enabled = enabled && DiagnosisTextbox.Text.Trim().Length > 0;
            enabled = enabled && PrescriptionTextbox.Text.Trim().Length > 0;
            enabled = enabled && DescriptionTextbox.Text.Trim().Length > 0;
            CreateRecordButton.IsEnabled = enabled;
        }

        private void CreateRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.IsEnabled = false;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    MedicalNumber medNum = new(int.Parse(PatientNumTextbox.Text.Replace(" ", "")));
                    string diagnosis = DiagnosisTextbox.Text.Trim();
                    string prescription = PrescriptionTextbox.Text.Trim();
                    string description = DescriptionTextbox.Text.Trim();

                    using NpgsqlCommand cmd = new();
                    cmd.Connection = Connection;
                    cmd.CommandText = "SELECT id FROM patients WHERE medical_num = @medical_num";
                    cmd.Parameters.Add(new NpgsqlParameter("medical_num", medNum));

                    if (!DbCalls.TryConvertMedicalNum(await cmd.ExecuteScalarAsync(), out int patientId))
                    {
                        throw new Exception("Unknown patient medical number.");
                    }

                    cmd.CommandText = "SELECT health_center.create_med_record(@patient_id, @doc_id, @diagnosis, @description, @prescription)";
                    cmd.Parameters.Add(new NpgsqlParameter("patient_id", patientId));
                    cmd.Parameters.Add(new NpgsqlParameter("doc_id", DoctorId));
                    cmd.Parameters.Add(new NpgsqlParameter("diagnosis", diagnosis) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    cmd.Parameters.Add(new NpgsqlParameter("description", description));
                    cmd.Parameters.Add(new NpgsqlParameter("prescription", prescription) { NpgsqlDbType = NpgsqlDbType.Varchar });
                    await cmd.ExecuteNonQueryAsync();

                    Close();
                }
                catch (Exception ex)
                {
                    ex.ToMessageBox();
                    UpdateButton();
                }
            });
        }
    }
}
