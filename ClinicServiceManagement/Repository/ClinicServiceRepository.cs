using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicServiceManagement.Repository
{
    public class ClinicServiceRepository : Repository<ClinicService>, IClinicServiceRepository
    {
        private readonly PetfriendsContext _context;
        public ClinicServiceRepository(PetfriendsContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<dynamic>> GetAllClinicService()
        {
            return await _context.ClinicServices
                .Include(c=>c.CategoryNavigation)
                .Select(c=> new
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryName = c.CategoryNavigation.Name,
                    Price = c.Price,
                    EstimateTime = c.EstimateTime,
                    Image = c.Image,
                }).ToListAsync();
        }
    }
}
