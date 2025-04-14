namespace PromotionManagementAPI.DTOs.PromotionDTOs
{
    public class PromotionListModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? TargetGroup { get; set; }

        public string CategoryName { get; set; }

        public int UsageLimit { get; set; }

        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }

    public class PromotionAddModel
    {
        public string? Name { get; set; }

        public byte? Type { get; set; }
        public decimal? DiscountDetail { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? TargetGroup { get; set; }

        public Guid? CategoryId { get; set; }

        public int UsageLimit { get; set; }

        public string? Description { get; set; }
    }

    public class PromotionUpdateModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public byte? Type { get; set; }
        public decimal? DiscountDetail { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? TargetGroup { get; set; }

        public Guid? CategoryId { get; set; }

        public int UsageLimit { get; set; }

        public string? Description { get; set; }
    }

    public class PromotionDetailModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public byte? Type { get; set; }
        public string TypeName => GetTypeName(Type);
        public decimal? DiscountDetail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TargetGroup { get; set; }
        public Guid? CategoryId { get; set; }
        public int UsageLimit { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
        private string GetTypeName(byte? type)
        {
            return type switch
            {
                0 => "Percentage",
                1 => "Amount",
                2 => "Free Service",
                _ => "Unknown"
            };
        }
    }
}
