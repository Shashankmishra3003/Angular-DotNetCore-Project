﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public virtual User Sender { get; set; }

        public int RecipientId { get; set; }

        public virtual User Recipient { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime? DateRead { get; set; }

        public DateTime MessageSend { get; set; }

        public bool SenderDeleted { get; set; }

        public bool RecipientDeleted { get; set; }
    }
}
