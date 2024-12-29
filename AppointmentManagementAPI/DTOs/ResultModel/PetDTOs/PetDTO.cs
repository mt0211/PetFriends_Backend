namespace AppointmentManagementAPI.DTOs.ResultModel.PetDTOs
{
    public class PetResponseModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Species { get; set; }
    }
}
