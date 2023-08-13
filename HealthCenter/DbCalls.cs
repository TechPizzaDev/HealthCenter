using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace HealthCenter
{
    public static class DbCalls
    {
        public static byte[] MakePassword(string password)
        {
            return SHA512.HashData(Encoding.UTF8.GetBytes(password));
        }

        public static async Task<int> Auth(NpgsqlConnection connection, MedicalNumber medNumber, byte[] password)
        {
            using NpgsqlCommand cmd = new();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT health_center.auth_user(@password, @med_num)";
            cmd.Parameters.Add(new NpgsqlParameter("med_num", medNumber));
            cmd.Parameters.Add(new NpgsqlParameter("password", password));

            object? result = await cmd.ExecuteScalarAsync();
            if (result != DBNull.Value && result is IConvertible convertible)
            {
                return convertible.ToInt32(null);
            }

            throw new InvalidCredentialException("The given credentials are not valid.");
        }
    }
}
