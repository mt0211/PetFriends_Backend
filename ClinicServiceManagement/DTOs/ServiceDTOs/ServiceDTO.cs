namespace ClinicServiceManagement.DTOs.ServiceDTOs
{
    public class ServiceListDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string EstimateTime { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string Image { get; set; }
    }

    public class ServiceAddDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? Category { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
        public string? EstimateTime { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? DiscountFrom { get; set; }
        public DateTime? DiscountTo { get; set; }
        public string? Image { get; set; }
    }
    public class ServiceDetailDTO
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
        public string? EstimateTime { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? DiscountFrom { get; set; }
        public DateTime? DiscountTo { get; set; }
        public string? Image { get; set; }
    }
}
