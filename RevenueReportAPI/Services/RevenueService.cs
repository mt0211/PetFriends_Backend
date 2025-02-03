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



        private List<ServiceRevenueDetailDTO> FormatDetailServiceRevenue(IEnumerable<dynamic> rawData, string timeFrame)
        {
            return timeFrame.ToLower() switch
            {
                "day" => rawData
                    .GroupBy(x => new
                    {
                        Date = (DateOnly)x.Date,
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDetailDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue),
                        Period = g.Key.Date.ToString("dd/MM/yyyy")
                    })
                    .OrderBy(x => x.Period)
                    .ThenBy(x => x.ServiceType)
                    .ToList(),

                "month" => rawData
                    .GroupBy(x => new
                    {
                        Year = ((DateOnly)x.Date).Year,
                        Month = ((DateOnly)x.Date).Month,
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDetailDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue),
                        Period = $"{g.Key.Month:00}/{g.Key.Year}"
                    })
                    .OrderBy(x => x.Period)
                    .ThenBy(x => x.ServiceType)
                    .ToList(),

                "year" => rawData
                    .GroupBy(x => new
                    {
                        Year = ((DateOnly)x.Date).Year,
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

                _ => throw new ArgumentException("Invalid TimeFrame")
            };
        }



        public async Task<ResultModel> GetDetailServiceRevenue(string token, RevenueRequestModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid user ID"
                };
            }

            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                string timeFrame = "year"; // Mặc định là tính theo năm

                if (request.Year.HasValue)
                {
                    if (request.Month.HasValue)
                    {
                        // Nếu có cả Year và Month → Lấy dữ liệu từng ngày trong tháng
                        startDate = new DateTime(request.Year.Value, request.Month.Value, 1);
                        endDate = startDate.Value.AddMonths(1).AddSeconds(-1);
                        timeFrame = "day"; // Lấy theo ngày
                    }
                    else
                    {
                        // Nếu chỉ có Year → Lấy dữ liệu từng tháng trong năm
                        startDate = new DateTime(request.Year.Value, 1, 1);
                        endDate = startDate.Value.AddYears(1).AddSeconds(-1);
                        timeFrame = "month"; // Lấy theo tháng
                    }
                }
                else
                {
                    return new ResultModel
                    {
                        IsSuccess = false,
                        Code = 400,
                        Message = "Year is required"
                    };
                }

                // Lấy dữ liệu từ database
                var rawData = await _revenueRepository.GetDetailServiceRevenue(startDate, endDate);
                var serviceRevenue = FormatDetailServiceRevenue(rawData, timeFrame);

                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new
                    {
                        Year = request.Year,
                        Month = request.Month,
                        Services = serviceRevenue
                    },
                    Message = "Successfully get data"
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 500,
                    ResponseFailed = ex.Message
                };
            }
        }

        private List<TotalRevenueDetailDTO> GenerateMonthlyTotalRevenue(IEnumerable<dynamic> rawData, int year)
        {
            var months = Enumerable.Range(1, 12).ToList();

            var groupedData = rawData
                .GroupBy(x => ((DateOnly)x.Date).Month)
                .ToDictionary(g => g.Key, g => g.Sum(x => (decimal)x.Revenue));

            return months.Select(m => new TotalRevenueDetailDTO
            {
                Time = m.ToString(),
                Revenue = groupedData.ContainsKey(m) ? groupedData[m] : 0
            }).ToList();
        }

        private List<TotalRevenueDetailDTO> GenerateDailyTotalRevenue(IEnumerable<dynamic> rawData, int year, int month)
        {
            var daysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month)).ToList();

            var groupedData = rawData
                .GroupBy(x => ((DateOnly)x.Date).Day)
                .ToDictionary(g => g.Key, g => g.Sum(x => (decimal)x.Revenue));

            return daysInMonth.Select(d => new TotalRevenueDetailDTO
            {
                Time = d.ToString(),
                Revenue = groupedData.ContainsKey(d) ? groupedData[d] : 0
            }).ToList();
        }
        public async Task<ResultModel> GetTotalRevenue(string token, RevenueRequestModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid user ID"
                };
            }

            try
            {
                if (!request.Year.HasValue)
                {
                    return new ResultModel
                    {
                        IsSuccess = false,
                        Code = 400,
                        Message = "Year is required"
                    };
                }

                var year = request.Year.Value;
                int? month = request.Month;

                // Lấy dữ liệu doanh thu từ repository
                var rawData = await _revenueRepository.GetTotalRevenue(year, month);

                List<TotalRevenueDetailDTO> totalRevenue;

                if (month.HasValue)
                {
                    // Nếu có tháng, lấy dữ liệu theo từng ngày
                    totalRevenue = GenerateDailyTotalRevenue(rawData, year, month.Value);
                }
                else
                {
                    // Nếu không có tháng, lấy dữ liệu theo từng tháng
                    totalRevenue = GenerateMonthlyTotalRevenue(rawData, year);
                }

                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new
                    {
                        Year = request.Year,
                        Month = request.Month,
                        Data = totalRevenue
                    },
                    Message = "Successfully get data"
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 500,
                    ResponseFailed = ex.Message
                };
            }
        }
        private List<ServiceRevenueDTO> FormatServiceRevenue(IEnumerable<dynamic> rawData, string timeFrame)
        {
            return timeFrame.ToLower() switch
            {
                "day" => rawData  // Thêm case xử lý cho "day"
                       .GroupBy(x => new
                       {
                           x.ServiceType
                       })
                       .Select(g => new ServiceRevenueDTO
                       {
                           ServiceType = g.Key.ServiceType,
                           Revenue = g.Sum(x => (decimal)x.Revenue)
                       })
                       .OrderBy(x => x.ServiceType)
                       .ToList(),
                "month" => rawData
                    .GroupBy(x => new
                    {
                        Year = ((DateOnly)x.Date).Year,
                        Month = ((DateOnly)x.Date).Month,
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue)
                    })
                    .OrderBy(x => x.ServiceType)
                    .ToList(),
                "year" => rawData
                    .GroupBy(x => new
                    {
                        Year = ((DateOnly)x.Date).Year,
                        x.ServiceType
                    })
                    .Select(g => new ServiceRevenueDTO
                    {
                        ServiceType = g.Key.ServiceType,
                        Revenue = g.Sum(x => (decimal)x.Revenue)
                    })
                    .OrderBy(x => x.ServiceType)
                    .ToList(),
                _ => throw new ArgumentException("Invalid TimeFrame")
            };
        }

        public async Task<ResultModel> GetServiceRevenue(string token, RevenueRequestModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid user ID"
                };
            }

            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                string timeFrame = "year"; // Mặc định là tính theo năm

                if (request.Year.HasValue)
                {
                    if (request.Month.HasValue)
                    {
                        // Nếu có cả Year và Month → Lấy dữ liệu từng ngày trong tháng
                        startDate = new DateTime(request.Year.Value, request.Month.Value, 1);
                        endDate = startDate.Value.AddMonths(1).AddSeconds(-1);
                        timeFrame = "day"; // Lấy theo ngày
                    }
                    else
                    {
                        // Nếu chỉ có Year → Lấy dữ liệu từng tháng trong năm
                        startDate = new DateTime(request.Year.Value, 1, 1);
                        endDate = startDate.Value.AddYears(1).AddSeconds(-1);
                        timeFrame = "month"; // Lấy theo tháng
                    }
                }
                else
                {
                    return new ResultModel
                    {
                        IsSuccess = false,
                        Code = 400,
                        Message = "Year is required"
                    };
                }

                // Lấy dữ liệu từ database
                var rawData = await _revenueRepository.GetServiceRevenue(startDate, endDate);
                var serviceRevenue = FormatServiceRevenue(rawData, timeFrame);

                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new
                    {
                        Year = request.Year,
                        Month = request.Month,
                        Services = serviceRevenue
                    },
                    Message = "Successfully get data"
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 500,
                    ResponseFailed = ex.Message
                };
            }
        }
    }
}
