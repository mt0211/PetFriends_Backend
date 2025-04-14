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
                .Where(c => c.CategoryNavigation.Status == 1 && c.IsBlocked == 1 && c.Status == "ACTIVE" || c.Status == "INACTIVE")
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.CreateAt,
                    CategoryName = c.CategoryNavigation.Name,
                    c.Price,
                    c.Status,
                    c.DiscountedPrice,
                    c.EstimateTime,
                    c.Image,
                }).ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetAllCategory()
        {
            return await _context.Categories
            .Where(c=>c.Status==1)
            .ToListAsync();
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
            _context.Entry(service).Property(s => s.IsDiscountNotified).IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoti(ClinicService service)
        {
            _context.ClinicServices.Attach(service);
            _context.Entry(service).Property(s => s.IsDiscountNotified).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task AddService(ClinicService service)
        {
            var sql = @"INSERT INTO ClinicService (Id, Name, Description, CreateAt, Category, Price, Status, 
                EstimateTime, DiscountAmount, DiscountFrom, DiscountTo, Image, IsBlocked, IsDiscountNotified) 
                VALUES (@Id, @Name, @Description, @CreateAt, @Category, @Price, @Status,
                @EstimateTime, @DiscountAmount, @DiscountFrom, @DiscountTo, @Image, @IsBlocked, @IsDiscountNotified)";
            var parameters = new[] 
            {
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
                new SqlParameter("@Image", service.Image ?? (object)DBNull.Value),
                new SqlParameter("@IsBlocked", 1),
                new SqlParameter("@IsDiscountNotified", service.IsDiscountNotified)
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }


        public async Task<ClinicService> GetServiceByNameAndCategory(string name, Guid? categoryId)
        {
            return await _context.ClinicServices.FirstOrDefaultAsync(s => s.Name == name && s.Category == categoryId);
        }

        public async Task<List<Vaccine>> GetAllVaccine()
        {
            return await _context.Vaccines
            .Where(v => v.Status == 0)
            .ToListAsync();
        }

        public async Task<ClinicService> GetServiceByName(string name)
        {
            return await _context.ClinicServices.FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Vaccine> GetVaccineByName(string name)
        {
            return await _context.Vaccines.FirstOrDefaultAsync(v => v.Name == name);
        }

        public async Task<Category> GetCategoryByID(Guid? id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<User>> GetListAdminAccount()
        {
            return await _context.Users.Where(u => u.Role == "ADMIN").ToListAsync();
        }

        public async Task<List<ClinicService>> GetServicesWithExpiredDiscount(DateTime currentTime)
        {
            return await _context.ClinicServices
            .Where(s => s.DiscountTo != null
            && s.DiscountTo < currentTime
            && s.DiscountAmount > 0
            && s.Status == "ACTIVE"
            && s.IsBlocked == 1
            && (s.IsDiscountNotified == null || s.IsDiscountNotified == false))
            .ToListAsync();
        }

    }
}
