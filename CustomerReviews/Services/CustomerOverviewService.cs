using CustomerReviews.DTOs.FeedbackDTOs;
using CustomerReviews.DTOs.RatingDTOs;
using CustomerReviews.DTOs.ResultModel;
using CustomerReviews.Repositories;
using CustomerReviews.Utilities;

namespace CustomerReviews.Services
{
    public class CustomerOverviewService : ICustomerOverviewService
    {
        private readonly ICustomerOverviewRepositiory _repository;
        public CustomerOverviewService(ICustomerOverviewRepositiory repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetListReviews(string token, int page)
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
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var feedbacks = await _repository.GetAllFeedback();
                if (page == 0)
                {
                    page = 1;
                }
                var feedbackList = feedbacks.Select(f => new FeedbackListResponseModel
                {
                    Id = f.Id,
                    UserName = f.UserName,
                    Content = f.Content,
                    CreatedAt = f.CreatedAt,
                    Rating = f.Rating,
                }).ToList();
                var paginatedResult = await Pagination.GetPagination(feedbackList, page, 10);
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

        public async Task<ResultModel> GetRatingOverView(string token)
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
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var (oneStarCount, twoStarCount, threeStarCount, fourStarCount, fiveStarCount, totalRating, agvRating) = await _repository.GetRating();
                var getDataCount = new RatingDTO
                {
                    oneStarCount = oneStarCount,
                    twoStarCount = twoStarCount,
                    threeStarCount = threeStarCount,
                    fourStarCount = fourStarCount,
                    fiveStarCount = fiveStarCount,
                    totalRating = totalRating,
                    avgRating = agvRating,
                };
                return new ResultModel
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = getDataCount, // Gán DTO vào ResultModel.Data
                    Message = "Successfully retrieved data rating"
                };

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
