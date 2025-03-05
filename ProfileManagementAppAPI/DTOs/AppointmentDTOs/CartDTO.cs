using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProfileManagementAppAPI.DTOs.AppointmentDTOs
{
    public class AddToCartDTO
    {
        public Guid ClinicServiceId { get; set; }
        public Guid PetId { get; set; }
    }
    public class CartDetailDTO
    {
        public Guid CartId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }
        public string UserAddress { get; set; }
        public List<CartServiceDTO> Services { get; set; } = new List<CartServiceDTO>();
        public DateTime? DateBook { get; set; }
        public string Notes { get; set; }
    }

    public class CartServiceDTO
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string EstimateTime { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public Guid PetId { get; set; }
        public string PetName { get; set; }
    }
    public class UpdateCartDTO
    {
        public Guid CartId { get; set; }
        public DateTime DateBook { get; set; }
        public string Notes { get; set; }
        public List<Guid> PromotionIds { get; set; } = new List<Guid>();
    }
    public class BookingResultDTO
    {
        public Guid AppointmentId { get; set; }
        public DateTime DateBook { get; set; }
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<AppliedPromotionDTO> AppliedPromotions { get; set; } = new List<AppliedPromotionDTO>();
        public List<CartServiceDTO> Services { get; set; }
    }

    public class AppliedPromotionDTO
    {
        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; }
        public byte? DiscountType { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}