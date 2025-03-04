using AdminServiceManagement.DTOs.ResultModel.CustomerReviews.DTOs.ResultModel;
using AdminServiceManagement.DTOs.ServiceDTOs;
using AdminServiceManagement.Repositories;
using AdminServiceManagement.Utilities;
using DataAccess.Models;

namespace AdminServiceManagement.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;
        public ServiceService(IServiceRepository repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetAllService(string token, int page)
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
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var services = await _repository.GetAllService();
                if (page == 0)
                {
                    page = 1;
                }
                var serviceList = services.Select(s=> new ServiceListResponseModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    CategoryId = s.CategoryId,
                    CategoryName = s.CategoryName,
                    Price = s.Price,
                    Status = s.Status,
                    EstimateTime = s.EstimateTime,
                    DiscountAmount = s.DiscountAmount,
                    DiscountFrom = s.DiscountFrom,
                    DiscountTo = s.DiscountTo,
                    Image = s.Image,
                    DiscountedPrice = s.DiscountedPrice,
                    IsBlocked = s.IsBlocked
                }).ToList();
                var paginatedResult = await Pagination.GetPagination(serviceList, page, 1000);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = paginatedResult;
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

        public async Task<ResultModel> UpdateServiceStatus(string token, ServiceUpdateRequestModel ServiceUpdateDTO)
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
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var newService = new ClinicService
                {
                    Id = ServiceUpdateDTO.Id,
                    IsBlocked = ServiceUpdateDTO.IsBlocked,
                };
                await _repository.UpdateStatus(newService);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = ServiceUpdateDTO;
                result.Message = "Successfully update service";
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
