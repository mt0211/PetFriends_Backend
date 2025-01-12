namespace RevenueReportAPI.DTOs.RevenueDTOs
{
    public class ServiceRevenueDTO
    {
        public string ServiceType { get; set; }
        public decimal Revenue { get; set; }
        public DateTime Date { get; set; }
    }
    public class ServiceRevenueDetailDTO
    {
        public string ServiceType { get; set; }
        public decimal Revenue { get; set; }
        public string Period { get; set; }  // Để hiển thị ngày/tháng/năm
    }
}
