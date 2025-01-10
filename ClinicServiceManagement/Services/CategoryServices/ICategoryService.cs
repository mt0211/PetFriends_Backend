using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagementAPI.DTOs.CategoryDTOs;

namespace ClinicServiceManagementAPI.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<ResultModel> AddNewService(string token, CategoryAddModel newcategory);
        Task<ResultModel> UpdateCategoryName(string token, CategoryUpdateModel newcategory);
        Task<ResultModel> GetListCategory(string token);
    }
}
