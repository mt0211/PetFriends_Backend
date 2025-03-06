using CustomerReviews.DTOs.ResultModel;

namespace CustomerReviews.Services
{
    public interface ICustomerOverviewService
    {
        Task<ResultModel> GetListReviews(string token);
        Task<ResultModel> GetRatingOverView(string token);
    }
}
