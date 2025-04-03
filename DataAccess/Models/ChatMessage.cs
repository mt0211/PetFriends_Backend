using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ChatMessage
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public string? Content { get; set; }

    public DateTime SentTime { get; set; }

    public bool IsRead { get; set; }

    public string MessageType { get; set; } = null!;

    public string? MediaUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? IsDeleteForSender { get; set; }

    public bool? IsDeleteForReceiver { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
