using AppForumManamgement.DTOs.PostDTOs;
using AppForumManamgement.DTOs.ResultModel;
using AppForumManamgement.Repositories;
using AppForumManamgement.Utilities;
using DataAccess.Models;
using MySqlX.XDevAPI.Common;

namespace AppForumManamgement.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repository;
        private readonly IMessageBus _messageBus;
        public PostService(IPostRepository repository, IMessageBus messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
        }
        public async Task<ResultModel> GetListPost(string token)
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
                var post = await _repository.GetListPost(id);
                if (post == null || !post.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found any post";
                    return result;
                }
                var postList = post.Select(p=> new PostListResModel
                {
                    Id = p.Id,
                    UserName = p.UserName,
                    UserAvatarUrl = p.UserAvatarUrl,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    ImageUrl = p.ImageUrl,  
                    TotalComment = p.TotalComment,
                    LikeCount = p.LikeCount,
                    DislikeCount = p.DislikeCount,
                    UserReaction = p.UserReaction
                }).ToList();


                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = postList;
                result.Message = "Successfully get all post";
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
        public async Task<ResultModel> UpdatePostReaction(string token, PostReactionReqModel request)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                // Kiểm tra xem người dùng đã reaction bài viết này chưa
                var existingReaction = await _repository.GetUserReactionToPost(id, request.PostId);
                
                // Nếu đã reaction và yêu cầu là thêm reaction mới
                if (existingReaction != null && request.IsAdd)
                {
                    // Nếu loại reaction khác với yêu cầu hiện tại (ví dụ: đã like nhưng giờ muốn dislike)
                    if (existingReaction.IsLike != request.IsLike)
                    {
                        // Xóa reaction cũ
                        await _repository.RemoveUserReaction(existingReaction);
                        
                        // Tạo reaction mới
                        var newReaction = new UserPostReaction
                        {
                            Id = Guid.NewGuid(),
                            UserId = id,
                            PostId = request.PostId,
                            IsLike = request.IsLike,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _repository.AddUserReaction(newReaction);
                        
                        // Cập nhật số lượng like/dislike của bài viết
                        var updatedPost = await _repository.UpdatePostReaction(request.PostId, request.IsLike, true);
                        // Giảm số lượng reaction đối lập
                        updatedPost = await _repository.UpdatePostReaction(request.PostId, !request.IsLike, false);
                        
                        if (updatedPost == null)
                        {
                            result.IsSuccess = false;
                            result.Code = 404;
                            result.Message = "Post not found";
                            return result;
                        }
                        
                        result.IsSuccess = true;
                        result.Code = 200;
                        result.Data = new
                        {
                            PostId = updatedPost.Id,
                            LikeCount = updatedPost.LikeCount,
                            DislikeCount = updatedPost.DislikeCount,
                            UserReaction = request.IsLike ? "like" : "dislike"
                        };
                        result.Message = $"Successfully changed from {(request.IsLike ? "dislike to like" : "like to dislike")}";
                    }
                    else
                    {
                        // Nếu loại reaction giống với yêu cầu hiện tại, không làm gì cả
                        result.IsSuccess = true;
                        result.Code = 200;
                        result.Message = "You have already reacted to this post";
                        return result;
                    }
                }
                // Nếu đã reaction và yêu cầu là xóa reaction
                else if (existingReaction != null && !request.IsAdd)
                {
                    // Xóa reaction
                    await _repository.RemoveUserReaction(existingReaction);
                    
                    // Cập nhật số lượng like/dislike của bài viết
                    var updatedPost = await _repository.UpdatePostReaction(request.PostId, existingReaction.IsLike, false);
                    
                    if (updatedPost == null)
                    {
                        result.IsSuccess = false;
                        result.Code = 404;
                        result.Message = "Post not found";
                        return result;
                    }
                    
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = new PostReactionResModel
                    {
                        PostId = updatedPost.Id,
                        LikeCount = updatedPost.LikeCount,
                        DislikeCount = updatedPost.DislikeCount,
                        UserReaction = null
                    };
                    result.Message = $"Successfully removed {(existingReaction.IsLike ? "like" : "dislike")}";
                }
                // Nếu chưa reaction và yêu cầu là thêm reaction
                else if (existingReaction == null && request.IsAdd)
                {
                    // Tạo reaction mới
                    var newReaction = new UserPostReaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = id,
                        PostId = request.PostId,
                        IsLike = request.IsLike,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repository.AddUserReaction(newReaction);
                    
                    // Cập nhật số lượng like/dislike của bài viết
                    var updatedPost = await _repository.UpdatePostReaction(request.PostId, request.IsLike, true);
                    
                    if (updatedPost == null)
                    {
                        result.IsSuccess = false;
                        result.Code = 404;
                        result.Message = "Post not found";
                        return result;
                    }
                    
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = new
                    {
                        PostId = updatedPost.Id,
                        LikeCount = updatedPost.LikeCount,
                        DislikeCount = updatedPost.DislikeCount,
                        UserReaction = request.IsLike ? "like" : "dislike"
                    };
                    result.Message = $"Successfully added {(request.IsLike ? "like" : "dislike")}";
                    // Ví dụ khi người dùng like bài viết:
                    if (request.IsAdd && request.IsLike)
                    {
                        // Lấy thông tin chủ bài viết
                        var post = await _repository.Get(request.PostId);
                        if (post != null && post.UserId != id) // Chỉ gửi thông báo nếu người like khác chủ bài viết
                        {
                            _messageBus.PublicPostReactionNotification(
                                "POST_LIKE",
                                request.PostId,
                                id, // ID người like
                                post.UserId.Value // ID chủ bài viết
                            );
                        }
                    }

                    // Tương tự cho dislike:
                    if (request.IsAdd && !request.IsLike)
                    {
                        var post = await _repository.Get(request.PostId);
                        if (post != null && post.UserId != id)
                        {
                            _messageBus.PublicPostReactionNotification(
                                "POST_DISLIKE",
                                request.PostId,
                                id,
                                post.UserId.Value
                            );
                        }
                    }
                }
                // Nếu chưa reaction và yêu cầu là xóa reaction (trường hợp không hợp lệ)
                else
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "You have not reacted to this post yet";
                    return result;
                }
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
        public async Task<ResultModel> GetPostByID(string token, Guid pid)
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
               var post = await _repository.GetPostByID(pid, id);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = post;
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
        public async Task<ResultModel> CommentPost(string token, CommentPostReqModel addmodel)
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
               var comment = new ForumComment{
                Id = Guid.NewGuid(),
                PostId = addmodel.PostId,
                UserId = id,
                CommentContent = addmodel.CommentContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
               };
               await _repository.AddCommentPost(comment);
               var post = await _repository.Get(addmodel.PostId);
               if (post != null)
               {
                _messageBus.PublicPostReactionNotification(
                "POST_COMMENT",
                addmodel.PostId,
                id, // ID người comment
                post.UserId.Value // ID chủ bài viết
                );
               }
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully add comment";
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
        public async Task<ResultModel> AddPost(string token, AddPostReqModel addmodel)
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
               var newPost = new ForumPost
               {
                Id = Guid.NewGuid(),
                UserId = id,
                PostContent = addmodel.PostContent,
                ImageUrl = addmodel.ImageUrl,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
               };
               await _repository.Insert(newPost);
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully add post";
               _messageBus.PublistPostActivity
               (
                "POST_CREATED",
                newPost.Id
               );
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
        public async Task<ResultModel> GetUserByID(string token)
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
               var user = await _repository.GetUserByID(id);
               result.IsSuccess = true;
               result.Code = 200;
               result.Data = user;
               result.Message = "Successfully get user information";
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
        public async Task<ResultModel> DeletePost(string token, Guid pid)
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
               var post = await _repository.GetUserPostByUserID(id, pid);
               if(post){
                 var PostToDelete = await _repository.Get(pid);
                await _repository.Remove(PostToDelete);
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully delete post";
               }
               else{
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Can't delete this post";
               }
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
        public async Task<ResultModel> UpdatePost(string token, UpdatePostReqModel updateModel)
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
               var newPost = new ForumPost
               {
                Id = updateModel.PostId,
                PostContent = updateModel.PostContent,
                ImageUrl = updateModel.ImageUrl,
                UpdatedAt = DateTime.UtcNow
               };
               await _repository.UpdatePost(newPost);
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully update post";
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
        public async Task<ResultModel> GetPostDetailToUpdate(string token, Guid pid)
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
                var post = await _repository.GetUserPostByUserID(id, pid);
                if(post){
               var postDetail = await _repository.GetPostByID(pid, id);
               result.IsSuccess = true;
               result.Code = 200;
               result.Data = postDetail;
               result.Message = "Successfully get post by id";
                }else{
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can't get this post";
                }
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
        public async Task<ResultModel> GetPostListByUserId(string token)
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
               var postList = await _repository.GetListPostByUserId(id);
               result.IsSuccess = true;
               result.Code = 200;
               result.Data = postList;
               result.Message = "Successfully get post by id";
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
        
        public async Task<ResultModel> DeleteComment(string token, Guid cid)
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
               var comment = await _repository.GetCommentByID(cid);
                if(comment.UserId != id)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can't delete other user's comment";
                    return result;
                }
                await _repository.DeleteCoomment(cid);
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully deleted comment";
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
        public async Task<ResultModel> UpdateComment(string token, UpdateCommentReqModel updateCommentReqModel)
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
               var comment = await _repository.GetCommentByID(updateCommentReqModel.CommentId);
                if(comment.UserId != id)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can't update other user's comment";
                    return result;
                }
                var newComment = new ForumComment
                {
                    Id = updateCommentReqModel.CommentId,
                    CommentContent = updateCommentReqModel.CommentContent,
                    UpdatedAt = DateTime.Now
                };
                  await _repository.UpdateComment(newComment);
               result.IsSuccess = true;
               result.Code = 200;
               result.Message = "Successfully update comment";
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
