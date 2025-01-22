namespace AccountManagementAPI.DTOs.UserDTOs
{
    public class UserListModel
    {
        public Guid Id { get; set; }
        public string? Role { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
    }
    public class UserUpdateStatusModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public string? ReasonToBlock { get; set; }
    }

    public class UserAddModel
    {
        public string? PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
    }
    public class UserDetailModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? AvartarURL { get; set; }
    }
    public class UserUpdateModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        //public string? PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public string? Status { get; set; }
       // public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }
       // public string? Password { get; set; }
        public string? AvartarURL { get; set; }
    }
}
