using DataAccess.Models;
using DataAccess.Repositories;
using ForumManagementAPI.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ForumManagementAPI.Repositories
{
    public class ForumRepository : Repository<ForumPost>, IForumRepository
    {
        private readonly PetfriendsContext _context;
        public ForumRepository(PetfriendsContext context):base(context) 
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllForumPost()
        {
            return await _context.ForumPosts
                .Include(f=>f.User)
                .Include(f=>f.ForumComments)
                .Select(f => new
                {
                    Id = f.Id,
                    UserName = f.User.FullName,
                    PostContent = f.PostContent,
                    CreatedAt = f.CreatedAt,
                    TotalComment = f.ForumComments.Count(),
                    Status = f.Status == 2 ? "Approved" : f.Status == 1 ? "Pending" : f.Status == 0 ? "Rejected" : "Unknow"
                }).ToListAsync();
        }

        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<dynamic> GetPostByID(Guid id)
        {
            var postDetail = await _context.ForumPosts
                .Where(p=>p.Id == id)
                .Include(p=>p.User)
                .Include(p=>p.ForumComments)
                .ThenInclude(c=>c.User).Select (p=> new
                {
                    Id = p.Id,
                    UserName = p.User.FullName,
                    UserAvatarUrl = p.User.AvatarUrl,
                    PostContent = p.PostContent,
                    ImageUrl = p.ImageUrl,
                   CreatedAt = p.CreatedAt,
                   LikeCount = p.LikeCount,
                   DisLikeCount = p.DislikeCount,
                   TotalComment = p.ForumComments.Count(),
                    Status = p.Status == 2 ? "Approved" : p.Status == 1 ? "Pending" : p.Status == 0 ? "Rejected" : "Unknow",
                    Comments = p.ForumComments.Select(c => new
                    {
                        CommentId = c.Id,
                        CommentContent = c.CommentContent,
                        ComentedBy = c.User.FullName,
                        UserCommentAvatarUrl = c.User.AvatarUrl,
                        CommentCreatedAt = c.CreatedAt,
                    }).ToList()
                }).FirstOrDefaultAsync();

            return postDetail;
        }

        public async Task DeletePost(Guid PostID)
        {
            var post = await _context.ForumPosts.FindAsync(PostID);
            if (post != null)
            {
                _context.ForumPosts.Remove(post);
            }
            await _context.SaveChangesAsync();
        }
        
        public async Task<(string email, string commentcontent)> GetUserEmailByCommentID(Guid cid)
        {
            var email = await _context.ForumComments
                .Where(c=>c.Id == cid)
                .Include(c=>c.User)
                .Select(c => new
                {
                    c.User.Email,
                    c.CommentContent
                }).FirstOrDefaultAsync();
            return (email.Email, email.CommentContent);
        }

        public async Task DeleteComment(Guid PCommentID)
        {
            var comment = await _context.ForumComments.FindAsync(PCommentID);
            if (comment != null)
            {
                _context.ForumComments.Remove(comment);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(string email, string postcontent)> GetUserEmailByPostID(Guid pid)
        {
            var email = await _context.ForumPosts
            .Where(c=>c.Id == pid)
            .Include(p => p.User)
            .Select(c=> new
            {
                c.User.Email,
                c.PostContent
            }).FirstOrDefaultAsync();
            return (email.Email, email.PostContent);
        }
    }
}
