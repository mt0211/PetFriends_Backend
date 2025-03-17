using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceDemandAPI.DTOs.ResultModel;
using ServiceDemandAPI.Repositories;
using ServiceDemandAPI.Utilities;

namespace ServiceDemandAPI.Services
{
    public class ServiceDemandService : IServiceDemandService
    {
        private readonly IServiceDemandRepository _repository;
        public ServiceDemandService(IServiceDemandRepository repository)
        {
            _repository = repository;   
        }
        public async Task<ResultModel> GetAppointmentDemand(string token, string dayOfWeek, DateTime? referenceDate = null)
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
                // Validate day of week
                if (!Enum.TryParse<DayOfWeek>(dayOfWeek, true, out DayOfWeek day))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid day of week";
                    return result;
                }

                // Mặc định ngày tham chiếu là ngày hiện tại
                var today = DateTime.UtcNow.Date;

                // Lấy dữ liệu từ repository
                var demand = await _repository.GetAppointmentDemandByHour(day, today);

                // Xử lý trường hợp không có dữ liệu
                if (!demand.Any())
                {
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = new { Message = "No data available" };
                    return result;
                }

                // Tìm giờ cao điểm
                var peakHour = demand
                    .OrderByDescending(x => ((dynamic)x).Count)
                    .First();

                // Format response theo hàng ngang
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new
                {
                    DayOfWeek = dayOfWeek,
                    HourlyDemand = demand.Select(x => new
                    {
                        Hour = ((dynamic)x).Hour,
                        Count = ((dynamic)x).Count,
                        Period = ((dynamic)x).Period
                    }),
                    PeakHour = new
                    {
                        Hour = ((dynamic)peakHour).Hour,
                        Count = ((dynamic)peakHour).Count,
                        Status = GetBusyStatus(((dynamic)peakHour).Count)
                    }
                };
                result.Message = "Successfully retrieved appointment demand";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.Message;
            }
            return result;
        }

        private string GetBusyStatus(int count)
        {
            return count switch
            {
                <= 3 => "Not busy",
                <= 5 => "Moderately busy",
                _ => "Very busy"
            };
        }
    }
}