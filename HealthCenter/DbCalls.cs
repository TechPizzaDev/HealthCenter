using System;
using System.Data.Common;
using System.Security.Authentication;
using System.Threading.Tasks;
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

        public static async Task<int> RegisterDoctor(NpgsqlConnection connection, EmployeeNumber employeeNumber, string fullName, string phone, byte[] password)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.register_doctor(@employee_num, @full_name, @phone, @password)";
            cmd.Parameters.Add(new NpgsqlParameter("employee_num", employeeNumber));
            cmd.Parameters.Add(new NpgsqlParameter("full_name", fullName));
            cmd.Parameters.Add(new NpgsqlParameter("phone", phone));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));
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
    }
}
