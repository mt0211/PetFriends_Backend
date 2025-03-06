using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerReviews.Repositories
{
    public class CustomerOverviewRepositiory : Repository<Feedback>, ICustomerOverviewRepositiory
    {
        private readonly PetfriendsContext _context;
        public CustomerOverviewRepositiory(PetfriendsContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllFeedback()
        {
            return await _context.Feedbacks
                .Include(f=>f.User)
                .Select(f=> new
                {
                    Id=f.Id,
                    UserName = f.User.FullName,
                    UserImageUrl = f.User.AvatarUrl,
                    Content = f.Content,
                    CreatedAt = f.CreatedAt,
                    Rating = f.Rating,
                }).ToListAsync();
        }


        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        public async Task<(int oneStarCount, int twoStarCount, int threeStarCount, int fourStarCount, int fiveStarCount, int totalRating, double avgRating)> GetRating()
        {
            var oneStarCount = await _context.Feedbacks.CountAsync(f => f.Rating == 1);
            var twoStarCount = await _context.Feedbacks.CountAsync(f => f.Rating == 2);
            var threeStarCount = await _context.Feedbacks.CountAsync(f => f.Rating == 3);
            var fourStarCount = await _context.Feedbacks.CountAsync(f => f.Rating == 4);
            var fiveStarCount = await _context.Feedbacks.CountAsync(f => f.Rating == 5);

            var totalRating = oneStarCount + twoStarCount + threeStarCount + fourStarCount + fiveStarCount;
            var avgRating = totalRating > 0
                ? (double)(oneStarCount * 1 + twoStarCount * 2 + threeStarCount * 3 + fourStarCount * 4 + fiveStarCount * 5) / totalRating
                : 0;

            return (oneStarCount, twoStarCount, threeStarCount, fourStarCount, fiveStarCount, totalRating, avgRating);
        }

    }
}
