namespace RevenueReportAPI.DTOs.UserRevenueDTOs
{
    public class UserBookingSummaryResponseModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? NumOfBook { get; set; }
        public decimal? Amount { get; set; }
    }
}
