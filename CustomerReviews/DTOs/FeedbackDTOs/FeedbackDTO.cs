namespace CustomerReviews.DTOs.FeedbackDTOs
{
    public class FeedbackListResponseModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string UserImageUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Rating { get; set; }
    }
}
