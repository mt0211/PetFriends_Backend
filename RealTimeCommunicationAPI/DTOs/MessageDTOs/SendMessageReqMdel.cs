using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeCommunicationAPI.DTOs.MessageDTOs
{
    public class SendMessageReqMdel
    {
        public string Token { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string? MediaUrl { get; set; }
    }
}