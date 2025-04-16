namespace AppUserAuthenticationAPI.DTOs.AppUserDTOs
{
    public class UserReqModel
    {
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string? ConfirmPassword { get; set; }    
        public string? PhoneNumber { get; set; }
    }
    public class UserLoginReqModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }

    }
    public class UpdateUserFullNameAndPhoneNumber
    {
        public Guid Id { get; set; }
        public  string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
    
}
