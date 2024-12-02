namespace UserAuthenticationAPI.DTOs.UserDTOs;
public class UserResModel
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
public class UserLoginResModel
{
    public required UserResModel User { get; set; }

    public required string Token { get; set; }
}
public class UserVerifyOTPResModel
{
    public string Email { get; set; } = null!;
    public string OTPCode { get; set; } = null!;
}

public class UserEmailForSendOTP
{
    public string Email { get; set; } = null!;
}