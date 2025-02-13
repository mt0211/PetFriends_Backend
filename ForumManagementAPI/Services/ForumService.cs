using ForumManagementAPI.DTOs.ForumPostDTOs;
using ForumManagementAPI.DTOs.ResultModel;
using ForumManagementAPI.Repositories;
using ForumManagementAPI.Utilities;

namespace ForumManagementAPI.Services
{
    public class ForumService : IForumService
    {
        private readonly IForumRepository _repository;
        public ForumService(IForumRepository repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetAllForumPost(string token, int page)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var posts = await _repository.GetAllForumPost();
                if (page == 0)
                {
                    page = 1;
                }
                var postsList = posts.Select(p => new ForumPostListResponseModel
                {
                    Id = p.Id,
                    UserName = p.UserName,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    TotalComment = p.TotalComment,
                    Status = p.Status,
                }).ToList();
                var paginatedResult = await Pagination.GetPagination(postsList, page, 10);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = paginatedResult;
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
        public async Task<ResultModel> GetPostDetail(string token, Guid pid)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
               var post = await _repository.GetPostByID(pid);
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                 await _repository.DeletePost(pid);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Delete post successfully";
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var userinfor = await _repository.GetUserEmailByCommentID(cid);
                if (!string.IsNullOrWhiteSpace(userinfor.email)) 
                {
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "Notification.html");
                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{content}}", userinfor.commentcontent);
                    bool EmailSent = await Email.SendEmail(userinfor.email, "Violate community standards", Html);
                }
                else
                {
                    Console.WriteLine("Email does not exist. Skipping email notification.");
                }
                await _repository.DeleteComment(cid);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Delete comment successfully";
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
