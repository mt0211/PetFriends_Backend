using DataAccess.Models;
using RevenueReportAPI.DTOs.ResultModel;
using RevenueReportAPI.DTOs.RevenueDTOs;
using RevenueReportAPI.DTOs.UserRevenueDTOs;
using RevenueReportAPI.Repositories;
using RevenueReportAPI.Utilities;
using System.Drawing;
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
                    AvatarUrl = u.UserAvatar,
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

        public async Task<ResultModel> GetAllDataForExport(string token, int year, int? month)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }
            
            try
            {
                // Tính toán startDate và endDate dựa trên year và month
                DateTime startDate, endDate;
                
                if (month.HasValue && month.Value > 0)
                {
                    // Nếu có month, lấy dữ liệu của tháng đó
                    startDate = new DateTime(year, month.Value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                }
                else
                {
                    // Nếu không có month, lấy dữ liệu của cả năm
                    startDate = new DateTime(year, 1, 1);
                    endDate = new DateTime(year, 12, 31);
                }
                
                var (userbookingsummaries, revenues, servicerevenue) = await _revenueRepository.GetAllDataForExport(year, month, startDate, endDate);
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    //USER BOOKING SUMMARY///////////////////////
                    //Create new sheet and name
                    var GetUserBookingSummariesForExport = package.Workbook.Worksheets.Add("User Booking Summaries");
                    
                    // --- Title: Top Vaccine Report (row 1) ---
                    GetUserBookingSummariesForExport.Cells[1, 1].Value = "User Booking Summaries Report";
                    var titleRange = GetUserBookingSummariesForExport.Cells[1, 1, 1, 3];
                    titleRange.Merge = true;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.Size = 14;
                    titleRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRange.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                    // Thêm viền cho titleRange
                    titleRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    /// -- End of title style section --


                    //Header
                    GetUserBookingSummariesForExport.Cells[2, 1].Value = "User Name";
                    GetUserBookingSummariesForExport.Cells[2, 2].Value = "Total Booking";
                    GetUserBookingSummariesForExport.Cells[2, 3].Value = "Total Amount";

                    //STYLE FOR HEADER  
                    using (var range = GetUserBookingSummariesForExport.Cells[2, 1, 2, 3])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; 

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black); 
                    }

                    //DATA  
                    int row = 3;
                    foreach (var item in userbookingsummaries)
                    {
                        GetUserBookingSummariesForExport.Cells[row, 1].Value = item.UserName;
                        GetUserBookingSummariesForExport.Cells[row, 2].Value = item.NumberOfBook;
                        GetUserBookingSummariesForExport.Cells[row, 3].Value = item.Amount;
                        row++;
                    }

                    //Style for data
                    if (row > 3) 
                    {
                        var dataRange = GetUserBookingSummariesForExport.Cells[2, 1, row - 1, 3];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }
                    GetUserBookingSummariesForExport.Cells.AutoFitColumns();
                    var tablenewuser = GetUserBookingSummariesForExport.Tables.Add(GetUserBookingSummariesForExport.Cells[2, 1, row - 1, 3], "UserBookingSummaryTable");
                    tablenewuser.ShowHeader = true;
                    tablenewuser.ShowFilter = false;
                    tablenewuser.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                    //Total Revenue
                    var GetTotalRevenue = package.Workbook.Worksheets.Add("Total Revenue");

                     // --- Title: Top Vaccine Report (row 1) ---
                    GetTotalRevenue.Cells[1, 1].Value = "Total Revenue Report";
                    var titleRanges = GetTotalRevenue.Cells[1, 1, 1, 2];
                    titleRanges.Merge = true;
                    titleRanges.Style.Font.Bold = true;
                    titleRanges.Style.Font.Size = 14;
                    titleRanges.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRanges.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRanges.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRanges.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                    // Thêm viền cho titleRange
                    titleRanges.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRanges.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRanges.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    /// -- End of title style section --
                    GetTotalRevenue.Cells[2,1].Value = "Date";
                    GetTotalRevenue.Cells[2,2].Value = "Total Revenue";

                    using (var range = GetTotalRevenue.Cells[2, 1, 2, 2])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; 

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black); 
                    }

                    int rowdatarevenue = 3;
                    foreach (var item in revenues)
                    {
                        GetTotalRevenue.Cells[rowdatarevenue, 1].Value = item.Date;
                        GetTotalRevenue.Cells[rowdatarevenue, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                        GetTotalRevenue.Cells[rowdatarevenue, 2].Value = item.Revenue;
                        rowdatarevenue++;
                    }
                    if (rowdatarevenue > 3)
                    {
                        var dataRange = GetTotalRevenue.Cells[3, 1, rowdatarevenue - 1, 2];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }
                    GetTotalRevenue.Cells.AutoFitColumns();
                    var tablenewusers = GetTotalRevenue.Tables.Add(GetTotalRevenue.Cells[2, 1, rowdatarevenue - 1, 2], "NewUsersTable");
                    tablenewusers.ShowHeader = true;
                    tablenewusers.ShowFilter = false;
                    tablenewusers.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                    //Service Revenue
                    var GetServiceRevenue = package.Workbook.Worksheets.Add("Service Revenue");

                     // --- Title: Top Vaccine Report (row 1) ---
                   GetServiceRevenue.Cells[1, 1].Value = "Service Revenue Report";
                    var titleRangesss = GetServiceRevenue.Cells[1, 1, 1, 3];
                    titleRangesss.Merge = true;
                    titleRangesss.Style.Font.Bold = true;
                    titleRangesss.Style.Font.Size = 14;
                    titleRangesss.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    titleRangesss.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    titleRangesss.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    titleRangesss.Style.Font.Color.SetColor(System.Drawing.Color.Black);


                    // Viền cho title
                    titleRangesss.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    titleRangesss.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;


                    titleRangesss.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    titleRangesss.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    /// -- End of title style section --

                    GetServiceRevenue.Cells[2,1].Value = "Date";
                    GetServiceRevenue.Cells[2,2].Value = "Service Revenue";
                    GetServiceRevenue.Cells[2,3].Value = "Service Name";
                    using (var range = GetServiceRevenue.Cells[2, 1, 2, 3])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#f2f4f4"));
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1f618d"));

                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin; 

                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black); 
                    }

                    int rowservicerevenue = 3;
                    foreach (var item in servicerevenue)
                    {
                        GetServiceRevenue.Cells[rowservicerevenue, 1].Value = item.Date;
                        GetServiceRevenue.Cells[rowservicerevenue, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                        GetServiceRevenue.Cells[rowservicerevenue, 2].Value = item.Revenue;
                        GetServiceRevenue.Cells[rowservicerevenue, 3].Value = item.ServiceType;
                        rowservicerevenue++;
                    }
                    
                    if (rowservicerevenue > 3)
                    {
                        var dataRange = GetServiceRevenue.Cells[3, 1, rowservicerevenue - 1, 3];
                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        dataRange.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        dataRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    }
                    GetServiceRevenue.Cells.AutoFitColumns();
                    var tableuserstatus = GetServiceRevenue.Tables.Add(
                        GetServiceRevenue.Cells[2, 1, rowservicerevenue - 1, 3],
                        "ServiceRevenueReport"
                    );
                    tableuserstatus.ShowHeader = true;
                    tableuserstatus.ShowFilter = false;
                    tableuserstatus.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                    //Convert to byte array and save to file
                    var fileBytes = package.GetAsByteArray();

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = Convert.ToBase64String(fileBytes);
                    result.Message = "Successfully exported all data to Excel";
                }
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
    }
}
