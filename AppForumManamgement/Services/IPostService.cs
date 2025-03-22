using AppForumManamgement.DTOs.PostDTOs;
using AppForumManamgement.DTOs.ResultModel;

namespace AppForumManamgement.Services
{
    public interface IPostService
    {
        Task<ResultModel> GetListPost(string token);
        Task<ResultModel> UpdatePostReaction(string token, PostReactionReqModel request);
        Task<ResultModel> GetPostByID(string token, Guid pid);
        Task<ResultModel> CommentPost(string token, CommentPostReqModel addmodel);
        Task<ResultModel> AddPost(string token, AddPostReqModel addmodel);
        Task<ResultModel> GetUserByID(string token);
        Task<ResultModel> DeletePost(string token, Guid pid);
        Task<ResultModel> GetPostDetailToUpdate(string token, Guid pid);
        Task<ResultModel> UpdatePost(string token, UpdatePostReqModel updateModel);
        Task<ResultModel> GetPostListByUserId(string token);
        Task<ResultModel> DeleteComment(string token, Guid cid);
        Task<ResultModel> UpdateComment(string token, UpdateCommentReqModel updateCommentReqModel);
        
    }
}
