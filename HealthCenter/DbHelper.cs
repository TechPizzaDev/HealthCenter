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
    }
}
