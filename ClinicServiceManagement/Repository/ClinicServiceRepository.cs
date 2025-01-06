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

        public async Task<ClinicService> GetServiceStatusByID(Guid id)
        {
            return await _context.ClinicServices
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync(); // Trả về trực tiếp model ClinicService
        }

       

        public async Task UpdateStatus(ClinicService service)
        {
            _context.ClinicServices.Attach(service);
            _context.Entry(service).Property(s => s.Status).IsModified = true; // Chỉ cập nhật cột Status
            await _context.SaveChangesAsync();
        }

        public async Task UpdateService(ClinicService service)
        {
            _context.ClinicServices.Attach(service);

            // Chỉ cập nhật các cột không phải cột tính toán
            _context.Entry(service).Property(s => s.Name).IsModified = true;
            _context.Entry(service).Property(s => s.Description).IsModified = true;
            _context.Entry(service).Property(s => s.Price).IsModified = true;
            _context.Entry(service).Property(s => s.Status).IsModified = true;
            _context.Entry(service).Property(s => s.Category).IsModified = true;
            _context.Entry(service).Property(s => s.EstimateTime).IsModified = true;
            _context.Entry(service).Property(s => s.DiscountAmount).IsModified = true;
            _context.Entry(service).Property(s => s.DiscountFrom).IsModified = true;
            _context.Entry(service).Property(s => s.DiscountTo).IsModified = true;
            _context.Entry(service).Property(s => s.Image).IsModified = true;

            await _context.SaveChangesAsync();
        }

    }
}
