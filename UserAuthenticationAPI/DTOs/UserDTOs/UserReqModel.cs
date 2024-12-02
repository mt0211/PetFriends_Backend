 namespace UserAuthenticationAPI.DTOs.UserDTOs;
 public class UserReqModel
    {
        public string? Email { get; set; }

        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? Gender { get; set; }

        public DateTime Dob { get; set; }

        public string? FullName { get; set; }


    }
    public class UserLoginReqModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }

    }
    public class ChangePasswordReqModel
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
    public class UserResetPasswordReqModel
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
    public class UserVerifyOTPReqModel
    {
        public string Email { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }
    public class UserResendOTPReqModel
    {
        public string? Email { get; set; }

    }
      public class UserUpdateModel
    {
        public Guid Id { get; set; }

      //  public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateTime? Dob { get; set; }

        public string? FullName { get; set; }
        public string AvatarUrl { get; set; }

    }
    public class UserUpdatePasswordModel
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;

    }
       public class EmailVerificationReqModel
    {
        public string OTP { get; set; }
        public string Email { get; set; }
    }