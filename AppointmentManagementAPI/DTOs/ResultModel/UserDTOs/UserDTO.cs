namespace AppointmentManagementAPI.DTOs.ResultModel.UserDTOs
{
    public class UserResponseModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
