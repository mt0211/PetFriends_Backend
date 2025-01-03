namespace ClinicServiceManagement.DTOs.ServiceDTOs
{
    public class ServiceListDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string EstimateTime { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
    }
}
