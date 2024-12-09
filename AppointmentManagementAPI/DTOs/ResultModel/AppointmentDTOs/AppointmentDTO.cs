namespace AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs
{
    public class AppointmentModel
    {
        public Guid Id { get; set; }
        public string? CustomerName {  get; set; }
        public string? PetName { get; set; }
        public DateTime? Date {  get; set; }    
        public DateTime? StartTime { get; set; }
        public string? ServiceType { get; set; }
        public string? Status { get; set; }
    }
    public class AppointmentUpdateStatusModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
    }
}
