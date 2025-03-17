using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace ServiceDemandAPI.Utilities
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
    }
}