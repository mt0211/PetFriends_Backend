using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;

namespace ProfileManagementAppAPI.DTOs.CategoryClinicServiceDTO
{
    public class CategoryListReqModel
    {
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public byte? CategoryStatus { get; set; }
        public List<ServiceListReqModel>? ClinicServices { get; set; }
    }

    public class ServiceListReqModel
    {
        public Guid ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public DateTime? ServiceCreateAt { get; set; }
        public decimal? ServicePrice { get; set; }
        public string? ServiceStatus { get; set; }
        public string? ServiceEstimateTime { get; set; }
        public decimal? ServiceDiscountAmount { get; set; }
        public DateTime? ServiceDiscountFrom { get; set; }
        public DateTime? ServiceDiscountTo { get; set; }
        public string? ServiceImage { get; set; }
        public decimal? ServiceDiscountedPrice { get; set; }
        public byte? ServiceIsBlocked { get; set; }

    }
}