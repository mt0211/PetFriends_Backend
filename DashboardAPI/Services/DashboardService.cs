using DashboardAPI.DTOs.DataDTOs;
using DashboardAPI.DTOs.ForumPostDTOs;
using DashboardAPI.DTOs.ResultModel;
using DashboardAPI.Repositories;
using DashboardAPI.Utilities;

namespace DashboardAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
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
            var user = await _dashboardRepository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            {
                
            }
            try
            {
                var (userCount, petCount, totalRevenue, totalPost, totalService) = await _dashboardRepository.GetDataCount();
                var getDataCount = new DataDTO
                {
                    
                    totalUser = userCount,
                    totalPet = petCount,
                    monthlyRevenue = totalRevenue,
                    totalPost = totalPost,
                    totalService = totalService
                    
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

        public async Task<ResultModel> GetForumPostStatistic(string token, DateTime? date)
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
            var user = await _dashboardRepository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Permission denied!";
                return result;
            }
            try
            {
                
                var (pendingCount, approvedCount, rejectCount ) = await _dashboardRepository.GetForumPostStatistic(date);
                var getDataCount = new ForumPostStatisticDTO
                {
                    //Date = date,
                    Pending = pendingCount,
                    Approved = approvedCount,
                    Rejected = rejectCount
                };
               

                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = getDataCount, // Gán DTO vào ResultModel.Data
                    Message = "Successfully retrieved forum post statistics"
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
