namespace RevenueReportAPI.DTOs.ResultModel
{
    public class ResultModel
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public object? Data { get; set; }
        public object? ResponseFailed { get; set; }
        public string? Message { get; set; }
    }
    public class RevenueRequestModel
    {
        public string TimeFrame { get; set; } = "day"; 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
