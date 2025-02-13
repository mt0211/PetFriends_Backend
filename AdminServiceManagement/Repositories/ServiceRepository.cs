using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdminServiceManagement.Repositories
{
    public class ServiceRepository : Repository<ClinicService>, IServiceRepository
    {
        private readonly PetfriendsContext _context;
        public ServiceRepository(PetfriendsContext context):base(context) 
        {
            _context = context;
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
        public async Task<IEnumerable<dynamic>> GetAllService()
        {
            return await _context.ClinicServices
                .Include(s=>s.CategoryNavigation)
                .Select(s=> new
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    CreatedAt = s.CreateAt,
                    CategoryId = s.Category,
                    CategoryName = s.CategoryNavigation.Name,
                    Price = s.Price,
                    Status = s.Status,
                    EstimateTime = s.EstimateTime,
                    DiscountAmount =s.DiscountAmount,
                    DiscountFrom = s.DiscountFrom,
                    DiscountTo = s.DiscountTo,
                    Image = s.Image,
                    DiscountedPrice = s.DiscountedPrice,
                    IsBlocked = s.IsBlocked
                }).ToListAsync();
        }
        public async Task UpdateStatus(ClinicService service)
        {
            _context.ClinicServices.Attach(service);
            _context.Entry(service).Property(s => s.IsBlocked).IsModified = true; 
            await _context.SaveChangesAsync();
        }

        
    }
}
