using DataAccess.Models;
using PromotionManagementAPI.DTOs.PromotionDTOs;
using PromotionManagementAPI.DTOs.ResultModel;
using PromotionManagementAPI.Repositories;
using PromotionManagementAPI.Utilities;

namespace PromotionManagementAPI.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        public PromotionService(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }
        public async Task<ResultModel> GetListPromotion(string token, int page)
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

            try
            {
                // Fetch raw data from the repository
                var promotions = await _promotionRepository.GetAllPromotion();

                if (promotions == null || !promotions.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "No appointments found";
                    return result;
                }

                if (page == 0)
                {
                    page = 1;
                }

                //// Transform entities to DTO
                var promotionList = promotions.Select(a => new PromotionListModel
                {
                    Id = a.Id, // Lấy Id từ kiểu ẩn danh
                    Name = a.Name,
                    Type = a.Type,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    TargetGroup = a.TargetGroup,
                    CategoryName = a.CategoryName,
                    UsageLimit = a.UsageLimit,
                    Status = a.Status,
                }).ToList();

                // Paginate the result
                var paginatedResult = await Pagination.GetPagination(promotionList, page, 10);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = paginatedResult;
                result.Message = "Successfully retrieved appointments";
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

        public async Task<ResultModel> GetAllCategory(string token)
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
            try
            {
                var vaccines = await _promotionRepository.GetAllCategory();
                if (vaccines == null || !vaccines.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found category";
                    return result;
                }
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccines;
                result.Message = "Successfully get all category";
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
        public async Task<ResultModel> DeletePromotion(string token, Guid pid)
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
            try
            {
                var promotion = await _promotionRepository.Get(pid);
                var vaccines = await _promotionRepository.Remove(promotion);

                result.IsSuccess = true;
                result.Code = 200;

                result.Message = "Successfully delete promotion";
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

        public async Task<ResultModel> AddPromotion(string token, PromotionAddModel promotionAddDTO)
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

            try
            {
                if (promotionAddDTO.Type == 0 && promotionAddDTO.DiscountDetail >= 100)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "DiscountDetail must be less than 100% for percentage promotion type";
                    return result;
                }
                var newPromotion = new Promotion
                {
                    Id = Guid.NewGuid(),
                    Name = promotionAddDTO.Name,
                    Type = promotionAddDTO.Type,
                    DiscountDetail = promotionAddDTO.DiscountDetail,
                    StartDate = promotionAddDTO.StartDate,
                    EndDate = promotionAddDTO.EndDate,
                    TargetGroup = promotionAddDTO.TargetGroup,
                    CategoryId = promotionAddDTO.CategoryId,
                    UsageLimit = promotionAddDTO.UsageLimit,
                    Description = promotionAddDTO.Description,
                };
            await _promotionRepository.AddNewPromotion(newPromotion);
            result.IsSuccess = true;
            result.Code = 200;
            result.Data = newPromotion;
            result.Message = "Successfully add new appointment";
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

        public async Task<ResultModel> UpdatePromotion(string token, PromotionUpdateModel promotionAddDTO)
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

            try
            {
                if (promotionAddDTO.Type == 0 && promotionAddDTO.DiscountDetail >= 100)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "DiscountDetail must be less than 100% for percentage promotion type";
                    return result;
                }
                var newPromotion = new Promotion
                {
                    Id = promotionAddDTO.Id,
                    Name = promotionAddDTO.Name,
                    Type = promotionAddDTO.Type,
                    DiscountDetail = promotionAddDTO.DiscountDetail,
                    StartDate = promotionAddDTO.StartDate,
                    EndDate = promotionAddDTO.EndDate,
                    TargetGroup = promotionAddDTO.TargetGroup,
                    CategoryId = promotionAddDTO.CategoryId,
                    UsageLimit = promotionAddDTO.UsageLimit,
                    Description = promotionAddDTO.Description,
                };
                await _promotionRepository.UpdatePromotion(newPromotion);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = newPromotion;
                result.Message = "Successfully add new appointment";
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

        public async Task<ResultModel> GetPromotionDetail(string token, Guid promotionId)
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

            try
            {
              var promotionDetail = await _promotionRepository.Get(promotionId);

                var promotionDetails = new PromotionDetailModel
                {
                    Name =  promotionDetail.Name,
                    Type = promotionDetail.Type,
                    DiscountDetail = promotionDetail.DiscountDetail,
                    StartDate = promotionDetail.StartDate,
                    EndDate = promotionDetail.EndDate,
                    Description = promotionDetail.Description,
                };
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = promotionDetails;
                result.Message = "Successfully add new appointment";
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
