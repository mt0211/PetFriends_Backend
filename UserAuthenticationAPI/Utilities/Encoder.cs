using DataAccess.DTOs.UserDTOs;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserAuthenticationAPI.Helpers;

namespace UserAuthenticationAPI.Utilities;

public class Encoder
{
    private static readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private static readonly AppConfig _appConfig;

    static Encoder()
    {
        _appConfig = ConfigurationHelper.LoadConfiguration();
        _tokenHandler = new JwtSecurityTokenHandler();
    }


    private static string GenerateSalt()
    {
        const int SaltLength = 16;
        byte[] salt = new byte[SaltLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToHexString(salt);
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

    public static bool VerifyPasswordHashed(string password, byte[] salt, byte[] storedPassword)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] combinedBytes = CombineBytes(passwordBytes, salt);
        byte[] newHash = HashPassword(combinedBytes);
        return storedPassword.AsSpan().SequenceEqual(newHash);
    }

    private static byte[] HashPassword(byte[] passwordCombined)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(passwordCombined);
    }

    private static byte[] CombineBytes(byte[] first, byte[] second)
    {
        var combined = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, combined, 0, first.Length);
        Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
        return combined;
    }

    public static string GenerateJWT(User user)
    {
        var key = _appConfig.JwtSettings.SecretKey;
        var issuer = _appConfig.JwtSettings.Issuer;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
            new Claim("userid", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("fullname", user.FullName ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public static string GenerateRandomPassword(int length = 12)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        using var rng = RandomNumberGenerator.Create();
        var result = new char[length];
        var buffer = new byte[sizeof(uint)];

        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            uint randomNumber = BitConverter.ToUInt32(buffer, 0);
            result[i] = chars[(int)(randomNumber % (uint)chars.Length)];
        }

        return new string(result);
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
}