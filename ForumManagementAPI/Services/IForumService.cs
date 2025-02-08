using ForumManagementAPI.DTOs.ResultModel;

namespace ForumManagementAPI.Services
{
    public interface IForumService
    {
        Task<ResultModel> GetAllForumPost(string token, int page);
        Task<ResultModel> GetPostDetail(string token, Guid pid);
        Task<ResultModel> DeletePost(string token, Guid pid);
        Task<ResultModel> DeleteComment(string token, Guid cid);
    }
}
