using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicServiceManagementAPI.Repository.ClinicServiceRepository
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
                .Where(c => c.CategoryNavigation.Status == 1)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    CategoryName = c.CategoryNavigation.Name,
                    c.Price,
                    c.DiscountedPrice,
                    c.EstimateTime,
                    c.Image,
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
                    s.Id,
                    s.Name,
                    s.Description,
                    CategoryName = s.CategoryNavigation.Name,
                    s.Price,
                    s.Status,
                    s.EstimateTime,
                    s.DiscountAmount,
                    s.DiscountFrom,
                    s.DiscountTo,
                    s.Image,
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

        public async Task AddService(ClinicService service)
        {
            var sql = @"INSERT INTO ClinicService (Id, Name, Description, CreateAt, Category, Price, Status, 
                EstimateTime, DiscountAmount, DiscountFrom, DiscountTo, Image) 
                VALUES (@Id, @Name, @Description, @CreateAt, @Category, @Price, @Status,
                @EstimateTime, @DiscountAmount, @DiscountFrom, @DiscountTo, @Image)";

            var parameters = new[] {
        new SqlParameter("@Id", service.Id),
        new SqlParameter("@Name", service.Name),
        new SqlParameter("@Description", service.Description ?? (object)DBNull.Value),
        new SqlParameter("@CreateAt", service.CreateAt),
        new SqlParameter("@Category", service.Category ?? (object)DBNull.Value),
        new SqlParameter("@Price", service.Price),
        new SqlParameter("@Status", service.Status),
        new SqlParameter("@EstimateTime", service.EstimateTime ?? (object)DBNull.Value),
        new SqlParameter("@DiscountAmount", service.DiscountAmount ?? (object)DBNull.Value),
        new SqlParameter("@DiscountFrom", service.DiscountFrom ?? (object)DBNull.Value),
        new SqlParameter("@DiscountTo", service.DiscountTo ?? (object)DBNull.Value),
        new SqlParameter("@Image", service.Image ?? (object)DBNull.Value)
    };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

    }
}
