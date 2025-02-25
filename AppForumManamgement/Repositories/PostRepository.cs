using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppForumManamgement.Repositories
{
    public class PostRepository : Repository<ForumPost> , IPostRepository
    {
        private readonly PetfriendsContext _context;
        public PostRepository(PetfriendsContext context) :base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetListPost()
        {
           return await _context.ForumPosts
                .Include(p=>p.User)
                .Include(p=>p.ForumComments)
                .Where(p=>p.Status == 2)
                .Select(p => new
                {
                    Id = p.Id,
                    UserName = p.User.FullName,
                    UserAvatarUrl = p.User.AvatarUrl,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    ImageUrl = p.ImageUrl,
                    TotalComment = p.ForumComments.Count(),
                    LikeCount = p.LikeCount,
                    DislikeCount = p.DislikeCount,
                }).ToListAsync();
                
        }

        public async Task<ForumPost?> UpdatePostReaction(Guid postId, bool isLike, bool isAdd)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null) return null;
            
            if (isLike)
            {
                post.LikeCount = isAdd ? (post.LikeCount ?? 0) + 1 : (post.LikeCount ?? 1) - 1;
            }
            else
            {
                post.DislikeCount = isAdd ? (post.DislikeCount ?? 0) + 1 : (post.DislikeCount ?? 1) - 1;
            }
            _context.ForumPosts.Attach(post);
            _context.Entry(post).Property(p => p.LikeCount).IsModified = true;
            _context.Entry(post).Property(p => p.DislikeCount).IsModified = true;
            await _context.SaveChangesAsync();
            return post;
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
        public async Task<ForumComment> AddCommentPost(ForumComment comment){
            await _context.ForumComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
        public async Task<User> GetUserByID(Guid id){
            return await _context.Users.FindAsync(id);
        }
        public async Task<bool> GetUserPostByUserID(Guid id, Guid pid){
            return await _context.ForumPosts.Where(p=>p.UserId == id && p.Id == pid).AnyAsync();
        }
        public async Task UpdatePost(ForumPost post)
        {
            _context.ForumPosts.Attach(post);
            _context.Entry(post).Property(p => p.PostContent).IsModified = true;
            _context.Entry(post).Property(p => p.ImageUrl).IsModified = true;
            _context.Entry(post).Property(p => p.UpdatedAt).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}
