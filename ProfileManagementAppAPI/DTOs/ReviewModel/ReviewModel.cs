namespace AppAppointmentManagementAPI.DTOs.ReviewModel
{
    public class ReviewModel
    {
        public Guid AppointmentId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        
    }
    public class ReviewUpdateModel
    {
        public Guid AppointmentId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }

    }

}
