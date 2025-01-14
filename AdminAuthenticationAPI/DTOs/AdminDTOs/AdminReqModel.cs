namespace AdminAuthenticationAPI.DTOs.AdminDTOs
{
    public class AdminReqModel
    {
        public class AdminLoginReqModel
        {
            public string? Email { get; set; }
            public string? Password { get; set; }

        }
    }
}
