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
    }
}
