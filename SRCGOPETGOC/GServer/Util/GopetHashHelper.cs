

using System.Security.Cryptography;
using System.Text;
using System;

namespace Gopet.Shared.Helper
{
    public class GopetHashHelper
    {
        public static string ComputeHash(string Text)
        {
            return BCrypt.Net.BCrypt.HashPassword(Text, 12);
        }

        public static bool VerifyHash(string Hash, string Text)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(Text, Hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Accounts created by older builds stored plaintext passwords. Keep those
                // accounts usable while all new registrations are stored as BCrypt hashes.
                return string.Equals(Hash, Text, StringComparison.Ordinal);
            }
        }
    }
}
