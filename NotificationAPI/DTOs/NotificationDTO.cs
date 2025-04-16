public class NotificationDTO
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}