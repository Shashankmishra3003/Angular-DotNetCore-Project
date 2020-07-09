using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1; // return first page if not requested

        // how many items to return to the client
        private int pageSize = 10;

        public int PageSize
        {
            get { return pageSize; }
            // returning only maximum allowed pages
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

        // used to match the sender ID
        public int UserId { get; set; }
        
        //messages received will be used to get recipient Id
        public string MessageContainer { get; set; } = "Unread";
    }
}
