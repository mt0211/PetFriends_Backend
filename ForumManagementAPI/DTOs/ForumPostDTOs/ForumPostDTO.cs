namespace ForumManagementAPI.DTOs.ForumPostDTOs
{
    public class ForumPostListResponseModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PostContent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int TotalComment {  get; set; }
        public string Status { get; set; }
    }
    public class ForumUpdateStatusRequestModel
    {
        public Guid Id { get; set; }
        public byte Status { get; set; }
    }
}
