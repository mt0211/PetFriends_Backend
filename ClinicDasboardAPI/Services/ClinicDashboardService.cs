using ClinicDasboardAPI.DTOs.AppointmentDTOs;
using ClinicDasboardAPI.DTOs.DataDTOs;
using ClinicDasboardAPI.DTOs.ResultModel;
using ClinicDasboardAPI.Repositories;
using ClinicDasboardAPI.Utilities;


namespace ClinicDasboardAPI.Services
{
    public class ClinicDashboardService : IClinicDashboardService
    {
        private readonly IClinicDashboardRepository _repository;
        public ClinicDashboardService(IClinicDashboardRepository repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetData(string token)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "PARTNER")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                var (userCount, petCount, totalRevenue, avgRating) = await _repository.GetDataCount();
                var getDataCount = new DataDTO
                {
                    totalUser = userCount,
                    totalPet = petCount,
                    monthlyRevenue = totalRevenue,
                    avgRating = avgRating
                };
                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = getDataCount;
                result.Message = "Successfully get data";
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

        public async Task<ResultModel> GetAppointmentStatistic(string token, DateTime? date)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400, // Bad request
                    Message = "Invalid user ID"
                };
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400, // Bad request
                    Message = "Please authorize"
                };
            }
            var user = await _repository.GetUserByID(id);
            if (user.Role != "PARTNER")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                var (pendingCount, confirmedCount, completedCount, canceledCount) = await _repository.GetAppointmentStatistic(date);
                var getDataCount = new AppointmentDTO
                {
                    //Date = date,
                    Pending = pendingCount,
                    Confirmed = confirmedCount,
                    Completed = completedCount,
                    Canceled = canceledCount
                };
                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = getDataCount, // Gán DTO vào ResultModel.Data
                    Message = "Successfully retrieved appointment statistics"
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 500,
                    ResponseFailed = ex.InnerException != null
                        ? ex.InnerException.Message + "\n" + ex.StackTrace
                        : ex.Message + "\n" + ex.StackTrace
                };
            }
        }
    }
}
