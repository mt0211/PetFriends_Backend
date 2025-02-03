using Microsoft.AspNetCore.Mvc;

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
       // [FromHeader(Name = "Year")] // Binding từ header
        public int? Year { get; set; } 

        //[FromHeader(Name = "Month")] // Binding từ header
        public int? Month { get; set; }
    }
}
