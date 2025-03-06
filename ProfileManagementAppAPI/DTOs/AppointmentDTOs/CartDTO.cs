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
        public List<Guid>? PromotionIds { get; set; } = new List<Guid>();
    }
    public class BookingResultDTO
    {
        public Guid AppointmentId { get; set; }
        public DateTime DateBook { get; set; }
        public string Notes { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
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

    public class BookingHistoryDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }
        public string PetName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartAt { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public List<BookingServiceDTO> Services { get; set; } = new List<BookingServiceDTO>();
    }

    public class BookingServiceDTO
    {
        public string ServiceName { get; set; }
        public string EstimateTime { get; set; }
        public decimal Price { get; set; }
        public string PetName { get; set; }
    }
    public class AppliedPromotionListDTO{
        public Guid? PromotionId { get; set; }

        public string? PromotionName { get; set; }

        public byte? PromotionType { get; set; }

        public DateTime? PromotionStartDate { get; set; }

        public DateTime? PromotionEndDate { get; set; }

        public string? PromotionTargetGroup { get; set; }

        public Guid? PromotionCategoryId { get; set; }

        public int? PromotionUsageLimit { get; set; }

        public string? PromotionStatus { get; set; }

        public string? PromotionDescription { get; set; }

        public decimal? PromotionDiscountDetail { get; set; }
    }
    public class UpdateAppointmentDTO
    {
        public Guid AppointmentId { get; set; }
        public DateTime? DateBook { get; set; }
        public string? Notes { get; set; }
        public List<Guid>? PromotionIds { get; set; }
         public List<Guid>? ServiceIds { get; set; }
    }
    public class AppointmentDetailDTO
    {
        public Guid AppointmentId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserEmail { get; set; }
        public string UserAddress { get; set; }
        public List<AppointmentServiceDTO> Services { get; set; } = new List<AppointmentServiceDTO>();
        public DateTime? DateBook { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    public class AppointmentServiceDTO
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string EstimateTime { get; set; }
        public decimal? Price { get; set; }
        public Guid PetId { get; set; }
        public string PetName { get; set; }
    }
    
}