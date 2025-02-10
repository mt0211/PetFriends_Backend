using DataAccess.Models;
using DataAccess.Repositories;

namespace CustomerReviews.Repositories
{
    public interface ICustomerOverviewRepositiory : IRepository<Feedback>
    {
        Task<User> GetUserByID(Guid id);
        Task<IEnumerable<dynamic>> GetAllFeedback();
        Task<(int oneStarCount, int twoStarCount, int threeStarCount, int fourStarCount, int fiveStarCount, int totalRating, double avgRating)> GetRating();
    }
}
