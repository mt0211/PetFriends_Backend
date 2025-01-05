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
                .Include(c => c.CategoryNavigation)
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryName = c.CategoryNavigation.Name,
                    Price = c.Price,
                    DiscountedPrice = c.DiscountedPrice,
                    EstimateTime = c.EstimateTime,
                    Image = c.Image,
                }).ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetAllCategory()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<dynamic> GetServiceByID(Guid id)
        {
            return await _context.ClinicServices
                .Include(s => s.CategoryNavigation)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    CategoryName = s.CategoryNavigation.Name,
                    Price = s.Price,
                    Status = s.Status,
                    EstimateTime = s.EstimateTime,
                    DiscountAmount = s.DiscountAmount,
                    DiscountFrom = s.DiscountFrom,
                    DiscountTo = s.DiscountTo,
                    Image = s.Image,
                }).FirstOrDefaultAsync();
        }

        public async Task UpdateDiscountedPrice(ClinicService service)
        {
            if (service.DiscountFrom.HasValue && service.DiscountTo.HasValue)
            {
                if (DateTime.UtcNow >= service.DiscountFrom.Value && DateTime.UtcNow <= service.DiscountTo.Value)
                {
                    service.DiscountedPrice = service.Price - (service.DiscountAmount ?? 0);
                }
                else
                {
                    service.DiscountedPrice = service.Price;
                }
            }
            else
            {
                service.DiscountedPrice = service.Price; // Không có giảm giá
            }

            _context.ClinicServices.Update(service);
            await _context.SaveChangesAsync();
        }

    }
}
