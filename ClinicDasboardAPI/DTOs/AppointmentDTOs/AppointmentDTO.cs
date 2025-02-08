namespace ClinicDasboardAPI.DTOs.AppointmentDTOs
{
    public class AppointmentDTO
    {
        public int Pending {  get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Canceled { get; set; }
    }
}
