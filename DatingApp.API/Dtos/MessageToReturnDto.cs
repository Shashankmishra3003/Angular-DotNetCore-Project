using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class MessageToReturnDto
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        //Auto mapper will map to KnownAs automatically
        public string SenderKnownAs { get; set; }

        public string SenderPhotoUrl { get; set; }

        public int RecipientId { get; set; }

        public string RecipientKnownAs { get; set; }

        public string RecipientPhotoUrl { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime? DateRead { get; set; }

        public DateTime MessageSend { get; set; }
    }
}
