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
    }
}
