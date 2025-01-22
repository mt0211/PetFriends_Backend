using AccountManagementAPI.DTOs.PasswordDTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace AccountManagementAPI.Utilities
{
    public class Encoder
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        static Encoder()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
        }
        public static string? DecodeToken(string jwtToken, string claimType)
        {
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(claimType))
                return null;
            try
            {
                var token = _tokenHandler.ReadJwtToken(jwtToken);
                return token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static CreateHashPasswordDTO CreateHashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            string saltString = GenerateSalt();
            byte[] salt = Convert.FromHexString(saltString);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedBytes = CombineBytes(passwordBytes, salt);
            byte[] hashedPassword = HashPassword(combinedBytes);

            return new CreateHashPasswordDTO
            {
                Salt = salt,
                HashedPassword = hashedPassword
            };
        }
        private static string GenerateSalt()
        {
            const int SaltLength = 16;
            byte[] salt = new byte[SaltLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return Convert.ToHexString(salt);
        }
        private static byte[] CombineBytes(byte[] first, byte[] second)
        {
            var combined = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, combined, 0, first.Length);
            Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
            return combined;
        }
        private static byte[] HashPassword(byte[] passwordCombined)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(passwordCombined);
        }
    }
}
