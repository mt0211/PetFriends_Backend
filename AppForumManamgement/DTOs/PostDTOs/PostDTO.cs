namespace AppForumManamgement.DTOs.PostDTOs
{
    public class PostListResModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string UserAvatarUrl { get; set; }
        public string PostContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }
        public int TotalComment {  get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
    }

    public class PostReactionReqModel
    {
        public Guid PostId { get; set; }
        public bool IsLike { get; set; }  
        public bool IsAdd { get; set; }   
    }
    public class CommentPostReqModel
    {
        public Guid PostId { get; set; }
        public string CommentContent { get; set; }
    }
    public class AddPostReqModel
    {
        public string PostContent { get; set; }
        public string ImageUrl { get; set; }
    }
    public class UpdatePostReqModel
    {
        public Guid PostId { get; set; }
        public string PostContent { get; set; }
        public string ImageUrl { get; set; }
    }

}
