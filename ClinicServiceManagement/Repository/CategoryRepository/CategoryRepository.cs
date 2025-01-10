using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicServiceManagementAPI.Repository.CategoryRepository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PetfriendsContext _context;
        public CategoryRepository(PetfriendsContext context) 
        {
            _context = context;
        }
        public async Task AddCategory(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdteCategoryName(Category category)
        {
            _context.Categories.Attach(category);
            _context.Entry(category).Property(c => c.Name).IsModified = true;
            _context.Entry(category).Property(c=>c.Status).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Category>> GetListCategory()
        {
            return await _context.Categories.ToListAsync();
        }
    }
}
