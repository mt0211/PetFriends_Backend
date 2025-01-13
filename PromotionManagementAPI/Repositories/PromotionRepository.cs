using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PromotionManagementAPI.Repositories
{
    public class PromotionRepository : Repository<Promotion>, IPromotionRepository
    {
        private readonly PetfriendsContext _context;
        public PromotionRepository(PetfriendsContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<dynamic>> GetAllPromotion()
        {
            return await _context.Promotions
                .Include(p=>p.Category)
                .Select(c=> new
                {
                    c.Id,
                    c.Name,
                    Type = c.Type == 0 ? "Percentage" : "Amount",
                    c.StartDate,
                    c.EndDate,
                    c.TargetGroup,
                    CategoryName = c.Category.Name,
                    c.UsageLimit,
                    c.Status,
                }).ToListAsync();
        }
    }
}
