using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminAuthenticationAPI.DTOs.GoogleLoginDTOs
{
    public class GoogleLoginDTO
    {
        public string Token { get; set; }
    }

    public class GoogleUserInfoDTO
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
    
}