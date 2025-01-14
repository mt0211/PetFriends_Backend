namespace AdminAuthenticationAPI.DTOs.AdminDTOs
{
    public class AdminRepModel
    {

        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime Dob { get; set; }

        public string Address { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Status { get; set; } = null!;

    }

    public class AdminLoginResModel
    {
        public required AdminRepModel User { get; set; }

        public required string Token { get; set; }
    }
    public class AdminVerifyOTPResModel
    {
        public string Email { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }

    public class AdminEmailForSendOTP
    {
        public string Email { get; set; } = null!;
    }
}
