using DataAccess.Models;
using RevenueReportAPI.DTOs.ResultModel;
using RevenueReportAPI.DTOs.RevenueDTOs;
using RevenueReportAPI.DTOs.UserRevenueDTOs;
using RevenueReportAPI.Repositories;
using RevenueReportAPI.Utilities;
using System.Runtime.CompilerServices;

namespace RevenueReportAPI.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IRevenueRepository _revenueRepository;
        public RevenueService(IRevenueRepository revenueRepository)
        {
            _revenueRepository = revenueRepository;
        }
        public async Task<ResultModel> GetUserBookingSummary(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var userSumary = await _revenueRepository.GetUserBookingSummaries();
                var userList = userSumary.Select(u => new UserBookingSummaryResponseModel
                {
                    Id = u.Id,
                    FullName = u.UserName,
                    NumOfBook = u.NumOfBook,
                    Amount = u.Amount,
                }).ToList();

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = userList;
                result.Message = "Successfully added new category";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }

        //Helper method
        private void SetDefaultDates(RevenueRequestModel request)
        {
            var today = DateTime.Now;

            switch (request.TimeFrame.ToLower())
            {
                case "day":
                    request.StartDate ??= today.Date;
                    request.EndDate ??= today.Date.AddDays(1).AddSeconds(-1);
                    break;

                case "month":
                    request.StartDate ??= new DateTime(today.Year, today.Month, 1);
                    request.EndDate ??= request.StartDate.Value.AddMonths(1).AddSeconds(-1);
                    break;

                case "year":
                    request.StartDate ??= new DateTime(today.Year, 1, 1);
                    request.EndDate ??= request.StartDate.Value.AddYears(1).AddSeconds(-1);
                    break;

                default:
                    throw new ArgumentException("TimeFrame không hợp lệ. Chỉ chấp nhận: day, month, year");
            }
        }

        private List<ServiceRevenueDetailDTO> FormatServiceRevenue(IEnumerable<dynamic> rawData, string timeFrame)
        {
            return timeFrame.ToLower() switch
            {
                "day" => rawData
                    .GroupBy(x => new {
                        Date = (DateOnly)x.Date, // Sử dụng DateOnly
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDetailDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue),
                        Period = g.Key.Date.ToString("dd/MM/yyyy") // Định dạng DateOnly
                    })
                    .OrderBy(x => x.Period)
                    .ThenBy(x => x.ServiceType)
                    .ToList(),

                "month" => rawData
                    .GroupBy(x => new {
                        Year = ((DateOnly)x.Date).Year, // Trích xuất Year từ DateOnly
                        Month = ((DateOnly)x.Date).Month, // Trích xuất Month từ DateOnly
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDetailDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue),
                        Period = $"{g.Key.Month:00}/{g.Key.Year}" // Định dạng tháng/năm
                    })
                    .OrderBy(x => x.Period)
                    .ThenBy(x => x.ServiceType)
                    .ToList(),

                "year" => rawData
                    .GroupBy(x => new {
                        Year = ((DateOnly)x.Date).Year, // Trích xuất Year từ DateOnly
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDetailDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue),
                        Period = g.Key.Year.ToString()
                    })
                    .OrderBy(x => x.Period)
                    .ThenBy(x => x.ServiceType)
                    .ToList(),

                _ => throw new ArgumentException("TimeFrame không hợp lệ")
            };
        }

        private List<TotalRevenueDetailDTO> FormatTotalRevenue(IEnumerable<dynamic> rawData, string timeFrame)
        {
            return timeFrame.ToLower() switch
            {
                "day" => rawData
                    .GroupBy(x => (DateOnly)x.Date) // Sử dụng DateOnly
                    .Select(g => new TotalRevenueDetailDTO
                    {
                        TotalAmount = g.Sum(x => (decimal)x.Revenue),
                        Period = g.Key.ToString("dd/MM/yyyy") // Định dạng DateOnly
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                "month" => rawData
                    .GroupBy(x => new {
                        Year = ((DateOnly)x.Date).Year, // Trích xuất Year từ DateOnly
                        Month = ((DateOnly)x.Date).Month // Trích xuất Month từ DateOnly
                    })
                    .Select(g => new TotalRevenueDetailDTO
                    {
                        TotalAmount = g.Sum(x => (decimal)x.Revenue),
                        Period = $"{g.Key.Month:00}/{g.Key.Year}" // Định dạng tháng/năm
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                "year" => rawData
                    .GroupBy(x => ((DateOnly)x.Date).Year) // Trích xuất Year từ DateOnly
                    .Select(g => new TotalRevenueDetailDTO
                    {
                        TotalAmount = g.Sum(x => (decimal)x.Revenue),
                        Period = g.Key.ToString()
                    })
                    .OrderBy(x => x.Period)
                    .ToList(),

                _ => throw new ArgumentException("TimeFrame không hợp lệ")
            };
        }

        public async Task<ResultModel> GetServiceRevenue(string token, RevenueRequestModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                // Thiết lập thời gian mặc định nếu không được chọn
                SetDefaultDates(request);

                var rawData = await _revenueRepository.GetServiceRevenue(request.StartDate, request.EndDate);
                var serviceRevenue = FormatServiceRevenue(rawData, request.TimeFrame);

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new
                {
                    TimeFrame = request.TimeFrame,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Services = serviceRevenue
                };
                result.Message = "Successfully get data";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.Message;
            }
            return result;
        }
        public async Task<ResultModel> GetTotalRevenue(string token, RevenueRequestModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                // Thiết lập thời gian mặc định nếu không được chọn
                SetDefaultDates(request);

                var rawData = await _revenueRepository.GetTotalRevenue(request.StartDate, request.EndDate);
                var totalRevenue = FormatTotalRevenue(rawData, request.TimeFrame);

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new
                {
                    TimeFrame = request.TimeFrame,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Revenue = totalRevenue
                };
                result.Message = "Successfully get data";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.Message;
            }
            return result;
        }
    }
}
