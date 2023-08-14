using System.Security.Cryptography;
using System.Text;

namespace HealthCenter
{
    public static class DbHelper
    {
        public static byte[] MakePassword(string password)
        {
            return SHA512.HashData(Encoding.UTF8.GetBytes(password));
        }

        public static string DoctorQuery(string selector)
        {
            return
                $"SELECT {selector} " +
                "FROM doctors d " +
                "INNER JOIN doc_special ds " +
                "ON d.employee_id = ds.doc_id " +
                "INNER JOIN specializations s " +
                "ON s.id = ds.special_id";
        }
    }
}
