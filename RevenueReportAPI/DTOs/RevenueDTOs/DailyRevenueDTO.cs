namespace RevenueReportAPI.DTOs.RevenueDTOs
{
    public class DailyRevenueDTO
    {
        public decimal TotalAmount { get; set; }
        public DateOnly Date { get; set; }
    }
    public class TotalRevenueDetailDTO
    {
        public string Time { get; set; }  // "1" → Tháng 1 hoặc Ngày 1
        public decimal Revenue { get; set; }
    }
}
