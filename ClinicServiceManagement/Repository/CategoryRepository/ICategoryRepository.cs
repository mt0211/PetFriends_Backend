using DataAccess.Models;

namespace ClinicServiceManagementAPI.Repository.CategoryRepository
{
    public interface ICategoryRepository
    {
        Task AddCategory(Category category);
        Task UpdteCategoryName(Category category);
        Task<IEnumerable<Category>> GetListCategory();
    }
}
