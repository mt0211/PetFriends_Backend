namespace CustomerReviews.DTOs.RatingDTOs
{
    public class RatingDTO
    {
        public int oneStarCount { get; set; }
        public int twoStarCount { get; set; }
          public int threeStarCount { get; set; }
        public int fourStarCount { get; set; }
            public int fiveStarCount { get;set; }
        public int totalRating { get; set; }
        public double avgRating { get; set; }
    }
}
