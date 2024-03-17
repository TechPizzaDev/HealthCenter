using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Npgsql;

namespace HealthCenter
{
    public static class DbCalls
    {
        /// <returns>The patient ID convertible to <see cref="MedicalNumber"/>.</returns>
        /// <exception cref="InvalidCredentialException"></exception>
        public static async Task<int> AuthPatient(
            NpgsqlConnection connection, MedicalNumber medNumber, byte[] password,
            CancellationToken cancellationToken = default)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT auth_patient(@med_num, @password)";
            cmd.Parameters.Add(new NpgsqlParameter("med_num", medNumber));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));

            object? result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (TryConvertMedicalNum(result, out int patientId))
            {
                return patientId;
            }
            throw new InvalidCredentialException("The given Patient credentials are not valid.");
        }

        public static bool TryConvertMedicalNum(object? result, out int value)
        {
            if (result != DBNull.Value && result is IConvertible convertible)
            {
                value = convertible.ToInt32(null);
                return true;
            }
            value = default;
            return false;
        }

        public static async Task<int> AuthEmployee(
            NpgsqlConnection connection, EmployeeNumber employeeNumber, byte[] password,
            CancellationToken cancellationToken = default)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT auth_employee(@employee_num, @password)";
            cmd.Parameters.Add(new NpgsqlParameter("employee_num", employeeNumber));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));

            object? result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result != DBNull.Value && result is IConvertible convertible)
            {
                return convertible.ToInt32(null);
            }

            throw new InvalidCredentialException("The given Employee credentials are not valid.");
        }

        public static async Task<int> RegisterDoctor(
            NpgsqlConnection connection, EmployeeNumber employeeNumber, string fullName, string phone, byte[] password, string specialization,
            CancellationToken cancellationToken = default)
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
                object? result = await cmd.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(result);
            }
            catch (DbException ex)
            {
                throw new InvalidCredentialException(ex.Message);
            }
        }

        public static async Task<bool> IsAdmin(
            NpgsqlConnection connection, int employeeId,
            CancellationToken cancellationToken = default)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.is_admin(@id)";
            cmd.Parameters.Add(new NpgsqlParameter("id", employeeId));
            object? result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToBoolean(result);
        }

        public static async Task<bool> IsDoctor(
            NpgsqlConnection connection, int employeeId,
            CancellationToken cancellationToken = default)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.is_doctor(@id)";
            cmd.Parameters.Add(new NpgsqlParameter("id", employeeId));
            object? result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToBoolean(result);
        }

        public static async Task<List<ScheduleHour>> GetDoctorSchedule(
            NpgsqlConnection connection, int employeeId, ScheduleHourChanged? changed,
            CancellationToken cancellationToken = default)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText =
                "SELECT hour, days FROM doc_schedule " +
                "WHERE (doc_id = @doc_id)";

            cmd.Parameters.Add(new NpgsqlParameter("doc_id", employeeId));

            List<(OffsetTime hour, BitArray days)> tuples = new();
            await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var hour = reader.GetFieldValue<OffsetTime>(0);
                    var days = reader.GetFieldValue<BitArray>(1);
                    tuples.Add((hour, days));
                }
            }

            BitArray?[] appointmentMasks = await GetAppointmentDayMasks(
                connection, employeeId, tuples.Select(x => x.hour), cancellationToken);

            List<ScheduleHour> hours = new();
            foreach (var (hour, scheduleDays) in tuples)
            {
                BitArray? mask = appointmentMasks[hours.Count];
                if (mask != null)
                {
                    scheduleDays.Xor(mask);
                }

                ScheduleHour item = new(hour, scheduleDays);
                item.Changed = changed;
                hours.Add(item);
            }
            return hours;
        }

        public static async Task<BitArray?[]> GetAppointmentDayMasks(
            NpgsqlConnection connection, int employeeId, IEnumerable<OffsetTime> hours,
            CancellationToken cancellationToken = default)
        {
            NpgsqlBatch batch = new(connection);

            foreach (OffsetTime hour in hours)
            {
                NpgsqlBatchCommand cmd = new();
                cmd.CommandText =
                    "SELECT day FROM appointments " +
                    "WHERE (doc_id = @doc_id AND hour = @hour)";

                cmd.Parameters.Add(new NpgsqlParameter("doc_id", employeeId));
                cmd.Parameters.Add(new NpgsqlParameter("hour", hour));

                batch.BatchCommands.Add(cmd);
            }

            await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync(cancellationToken);

            BitArray?[] masks = new BitArray?[batch.BatchCommands.Count];
            for (int i = 0; i < masks.Length; i++)
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var day = reader.GetFieldValue<BitArray>(0);

                    BitArray mask = masks[i] ?? (masks[i] = new BitArray(5));
                    mask.Or(day);
                }
                
                if (!await reader.NextResultAsync(cancellationToken))
                {
                    break;
                }
            }

            return masks;
        }
    }
}
