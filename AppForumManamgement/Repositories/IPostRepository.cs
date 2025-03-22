using DataAccess.Models;
using DataAccess.Repositories;
namespace AppForumManamgement.Repositories
{
    public interface IPostRepository : IRepository<ForumPost>
    {
        Task<IEnumerable<dynamic>> GetListPost();
        Task<ForumPost?> UpdatePostReaction(Guid postId, bool isLike, bool isAdd);
        Task<dynamic> GetPostByID(Guid id);
        Task<ForumComment> AddCommentPost(ForumComment comment);
        Task<User> GetUserByID(Guid id);
        Task<bool> GetUserPostByUserID(Guid id, Guid pid);
        Task UpdatePost(ForumPost post);
        Task<List<ForumPost>> GetListPostByUserId(Guid id);
        Task<ForumComment> GetCommentByID(Guid id);
        Task DeleteCoomment(Guid id);
        Task<ForumComment> UpdateComment(ForumComment comment);
    }
}
