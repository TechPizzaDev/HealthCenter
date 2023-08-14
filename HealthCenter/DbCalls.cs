using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows.Documents;
using NodaTime;
using Npgsql;

namespace HealthCenter
{
    public static class DbCalls
    {
        public static async Task<int> AuthPatient(NpgsqlConnection connection, MedicalNumber medNumber, byte[] password)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT auth_patient(@med_num, @password)";
            cmd.Parameters.Add(new NpgsqlParameter("med_num", medNumber));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));

            object? result = await cmd.ExecuteScalarAsync();
            if (result != DBNull.Value && result is IConvertible convertible)
            {
                return convertible.ToInt32(null);
            }

            throw new InvalidCredentialException("The given Patient credentials are not valid.");
        }

        public static async Task<int> AuthEmployee(NpgsqlConnection connection, EmployeeNumber employeeNumber, byte[] password)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT auth_employee(@employee_num, @password)";
            cmd.Parameters.Add(new NpgsqlParameter("employee_num", employeeNumber));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));

            object? result = await cmd.ExecuteScalarAsync();
            if (result != DBNull.Value && result is IConvertible convertible)
            {
                return convertible.ToInt32(null);
            }

            throw new InvalidCredentialException("The given Employee credentials are not valid.");
        }

        public static async Task<int> RegisterDoctor(
            NpgsqlConnection connection, EmployeeNumber employeeNumber, string fullName, string phone, byte[] password, string specialization)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.register_doctor(@employee_num, @full_name, @phone, @password, @spec)";
            cmd.Parameters.Add(new NpgsqlParameter("employee_num", employeeNumber));
            cmd.Parameters.Add(new NpgsqlParameter("full_name", fullName));
            cmd.Parameters.Add(new NpgsqlParameter("phone", phone));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));
            cmd.Parameters.Add(new NpgsqlParameter("spec", specialization));
            try
            {
                object? result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (DbException ex)
            {
                throw new InvalidCredentialException(ex.Message);
            }
        }

        public static async Task<bool> IsAdmin(NpgsqlConnection connection, int employeeId)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.is_admin(@id)";
            cmd.Parameters.Add(new NpgsqlParameter("id", employeeId));
            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public static async Task<bool> IsDoctor(NpgsqlConnection connection, int employeeId)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.is_doctor(@id)";
            cmd.Parameters.Add(new NpgsqlParameter("id", employeeId));
            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public static async Task<List<ScheduleHour>> GetDoctorSchedule(NpgsqlConnection connection, int employeeId, ScheduleHourChanged? changed)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText =
                "SELECT * FROM doc_schedule " +
                "WHERE (doc_id = @doc_id)";

            cmd.Parameters.Add(new NpgsqlParameter("doc_id", employeeId));

            List<(OffsetTime hour, BitArray days)> tuples = new();
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync(default))
                {
                    var hour = reader.GetFieldValue<OffsetTime>(1);
                    var days = reader.GetFieldValue<BitArray>(2);
                    tuples.Add((hour, days));
                }
            }

            List<ScheduleHour> hours = new();
            foreach (var (hour, scheduleDays) in tuples)
            {
                BitArray appointmentMask = await GetAppointmentDayMask(connection, employeeId, hour);
                ScheduleHour item = new(hour, scheduleDays.Xor(appointmentMask));
                item.Changed = changed;
                hours.Add(item);
            }
            return hours;
        }

        public static async Task<BitArray> GetAppointmentDayMask(NpgsqlConnection connection, int employeeId, OffsetTime hour)
        {
            BitArray mask = new(5); using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText =
                "SELECT day FROM appointments " +
                "WHERE (doc_id = @doc_id AND hour = @hour)";

            cmd.Parameters.Add(new NpgsqlParameter("doc_id", employeeId));
            cmd.Parameters.Add(new NpgsqlParameter("hour", hour));

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync(default))
            {
                var day = reader.GetFieldValue<BitArray>(0);
                mask.Or(day);
            }
            return mask;
        }
    }
}
