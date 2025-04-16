using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.SqlClient;
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
                .Include(p => p.Category)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Type = c.Type == 0 ? "Percentage" : c.Type == 1 ? "Amount" : "Free Service",
                    c.StartDate,
                    c.EndDate,
                    c.TargetGroup,
                    CategoryName = c.Category.Name,
                    c.UsageLimit,
                    c.CreatedAt,
                    c.Status,
                    
                }).ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetAllCategory()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task AddNewPromotion(Promotion promotion)
        {
            var sql = @"insert into Promotion (Id, Name, Type, StartDate, EndDate, TargetGroup, CategoryId, UsageLimit, Description, DiscountDetail, IsExpriedNotified, CreatedAt)
        values(@Id, @Name, @Type, @StartDate, @EndDate, @TargetGroup, @CategoryId, @UsageLimit, @Description, @DiscountDetail, @IsExpriedNotified, @CreatedAt)";
            var parameters = new[]
            {
            new SqlParameter("@Id", promotion.Id),
             new SqlParameter("@Name", promotion.Name),
              new SqlParameter("@Type", promotion.Type),
               new SqlParameter("@StartDate", promotion.StartDate),
                new SqlParameter("@EndDate", promotion.EndDate),
                 new SqlParameter("@TargetGroup", promotion.TargetGroup),
                  new SqlParameter("@CategoryId", promotion.CategoryId),
                   new SqlParameter("@UsageLimit", promotion.UsageLimit),
                     new SqlParameter("@Description", promotion.Description),
                     new SqlParameter("@DiscountDetail", promotion.DiscountDetail),
                     new SqlParameter("@IsExpriedNotified", promotion.IsExpriedNotified),
                     new SqlParameter("@CreatedAt", promotion.CreatedAt),
        };
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task UpdatePromotion(Promotion promotion)
        {
            _context.Promotions.Attach(promotion);
            _context.Entry(promotion).Property(s => s.Name).IsModified = true;
            _context.Entry(promotion).Property(s => s.Type).IsModified = true;
            _context.Entry(promotion).Property(s => s.StartDate).IsModified = true;
            _context.Entry(promotion).Property(s => s.EndDate).IsModified = true;
            _context.Entry(promotion).Property(s => s.TargetGroup).IsModified = true;
            _context.Entry(promotion).Property(s => s.CategoryId).IsModified = true;
            _context.Entry(promotion).Property(s => s.UsageLimit).IsModified = true;
            _context.Entry(promotion).Property(s => s.Description).IsModified = true;
            _context.Entry(promotion).Property(s => s.DiscountDetail).IsModified = true;
            _context.Entry(promotion).Property(s => s.IsExpriedNotified).IsModified = true;
            _context.Entry(promotion).Property(s => s.CreatedAt).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoti(Promotion promotion)
        {
            _context.Promotions.Attach(promotion);
            _context.Entry(promotion).Property(s => s.IsExpriedNotified).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Promotion>> GetExpiredPromotions(DateTime currentTime)
        {
            return await _context.Promotions
            .Where(p => p.EndDate != null
            && p.EndDate < currentTime
            && (p.IsExpriedNotified == null || p.IsExpriedNotified == false))
            .ToListAsync();
        }
        public async Task<Promotion> GetPromotionById(Guid id)
        {
            return await _context.Promotions
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Promotion> GetPromotionByName(string name)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.Name == name);
        }

    }
}
