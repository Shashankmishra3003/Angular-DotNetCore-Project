using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1; // return first page if not requested

        // how many items to return to the client
        private int pageSize = 10;

        public int PageSize
        {
            get { return pageSize; }
            // returning only maximum allowed pages
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value ; }
        }

        public int UserId { get; set; }

        public string Gender { get; set; }
        
        public int MinAge { get; set; } = 18;

        public int MaxAge { get; set; } = 99;

        public string Orderby { get; set; }

    }
}
