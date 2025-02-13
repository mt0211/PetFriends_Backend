using Org.BouncyCastle.Pkcs;

namespace AdminServiceManagement.DTOs.ServiceDTOs
{
    public class ServiceListResponseModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
        public string? EstimateTime { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? DiscountFrom { get; set; }
        public DateTime? DiscountTo { get; set; }
        public string? Image { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public byte? IsBlocked { get; set; }
    
    }
    public class ServiceUpdateRequestModel
    {
        public Guid Id { get; set; }
        public byte? IsBlocked { get; set; }
    }
}
