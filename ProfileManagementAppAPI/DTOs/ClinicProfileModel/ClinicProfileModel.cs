using DataAccess.Models;

namespace ProfileManagementAppAPI.DTOs.ClinicProfileModel
{
    public class ClinicProfileModel
    {
        public User User { get; set; }
        public  List<Category> Categories { get; set; }


    }


    public class UserUpdateModel
    {
        public Guid Id { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateTime? Dob { get; set; }

        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }

    }
}
