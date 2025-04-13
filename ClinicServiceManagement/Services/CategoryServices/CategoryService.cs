using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
using ClinicServiceManagement.Utilites;
using ClinicServiceManagementAPI.DTOs.CategoryDTOs;
using ClinicServiceManagementAPI.Repository.CategoryRepository;
using DataAccess.Models;
using System.Runtime.CompilerServices;

namespace ClinicServiceManagementAPI.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<ResultModel> AddNewCategory(string token, CategoryAddModel newcategory)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var categoryEntity = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = newcategory.Name,
                    Status = 1,
                };
                var category = await _categoryRepository.GetCategoryByName(newcategory.Name);
                if (category != null)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Category already exists";
                    return result;
                }

                await _categoryRepository.AddCategory(categoryEntity);

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = newcategory;
                result.Message = "Successfully added new category";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> UpdateCategoryName(string token, CategoryUpdateModel newcategory)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var categoryEntity = new Category
                {
                    Id = newcategory.Id,
                    Name = newcategory.Name,
                    Status = newcategory.Status,
                };
                var existingCategory = await _categoryRepository.GetCategoryByName(newcategory.Name);
                if (existingCategory != null && existingCategory.Id != newcategory.Id)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Category name already exists";
                    return result;
                }

                await _categoryRepository.UpdteCategoryName(categoryEntity);

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = categoryEntity;
                result.Message = "Successfully update cateogry";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetListCategory(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
               

              var categoryList =  await _categoryRepository.GetListCategory();

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = categoryList;
                result.Message = "Successfully update cateogry";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
    }
}
