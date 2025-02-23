namespace AppAppointmentManagementAPI.DTOs.ReviewModel
{
    public class ReviewModel
    {
        public string Content { get; set; }
        public int Rating { get; set; }
        
    }
    public class ReviewUpdateModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }

    }

}
