using DataAccess.Models;
using DataAccess.Repositories;

namespace ForumManagementAPI.Repositories
{
    public interface IForumRepository : IRepository<ForumPost>
    {
        Task<IEnumerable<dynamic>> GetAllForumPost();
        Task<User> GetUserByID(Guid id);
        Task<dynamic> GetPostByID(Guid id);
        Task DeletePost(Guid PostID);
        Task DeleteComment(Guid PCommentID);
        Task<(string email, string commentcontent)> GetUserEmailByCommentID(Guid cid);
    }
}
