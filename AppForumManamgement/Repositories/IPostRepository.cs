using DataAccess.Models;
using DataAccess.Repositories;
namespace AppForumManamgement.Repositories
{
    public interface IPostRepository : IRepository<ForumPost>
    {
        Task<IEnumerable<dynamic>> GetListPost(Guid userId);
        Task<ForumPost?> UpdatePostReaction(Guid postId, bool isLike, bool isAdd);
        Task<dynamic> GetPostByID(Guid id, Guid userId);
        Task<ForumComment> AddCommentPost(ForumComment comment);
        Task<User> GetUserByID(Guid id);
        Task<bool> GetUserPostByUserID(Guid id, Guid pid);
        Task UpdatePost(ForumPost post);
        Task<IEnumerable<dynamic>> GetListPostByUserId(Guid userId);
        Task<ForumComment> GetCommentByID(Guid id);
        Task DeleteCoomment(Guid id);
        Task<ForumComment> UpdateComment(ForumComment comment);

        //FIX API
        Task<UserPostReaction> GetUserReactionToPost(Guid userId, Guid postId);
        Task AddUserReaction(UserPostReaction reaction);
        Task RemoveUserReaction(UserPostReaction reaction);
        Task<bool> HasUserReactedToPost(Guid userId, Guid postId, bool? isLike = null);
    }
}
