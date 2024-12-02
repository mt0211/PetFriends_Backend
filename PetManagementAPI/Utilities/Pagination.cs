using PetManagementAPI.DTOs.ResultModel;

namespace PetManagementAPI.Utilities
{
    public class Pagination
    {
        public static async Task<PaginationModel<T>> GetPagination<T>(List<T> list, int page, int pageSize)
        {
            int StartIndex = (page - 1) * pageSize;
            int EndIndex = StartIndex + pageSize;
            var currentPageData = list.Skip(StartIndex).Take(pageSize).ToList();
            await Task.Delay(1); // Simulate an asynchronous operation (optional)
            var paginationModel = new PaginationModel<T>
            {
                Page = page,
                TotalRecords = list.Count,
                TotalPage = (int)Math.Ceiling(list.Count / (double)pageSize),
                ListData = currentPageData
            };
            return paginationModel;
        }
    }
}
