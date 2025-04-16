public class ForumPostEvent
{
   public string Type { get; set; }
    public Guid PostId { get; set; }
    public Guid ReactingUserId { get; set; }
    public Guid PostOwnerId { get; set; }
}