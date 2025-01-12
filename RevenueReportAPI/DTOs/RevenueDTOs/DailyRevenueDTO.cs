namespace RevenueReportAPI.DTOs.RevenueDTOs
{
    public class DailyRevenueDTO
    {
        public decimal TotalAmount { get; set; }
        public DateOnly Date { get; set; }
    }
    public class TotalRevenueDetailDTO
    {
        public decimal TotalAmount { get; set; }
        public string Period { get; set; }  // Để hiển thị ngày/tháng/năm
    }
}
