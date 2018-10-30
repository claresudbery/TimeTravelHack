using System;

namespace TimeTravelApi.Models
{
    public class MoreTimeRequest
    {
        public long Id { get; set; }
        public DateTime RequestTimeStamp { get; set; }
        public bool Expired {get; set;}
        public bool Alerted {get; set;}
        public int LengthInMinutes {get; set;}
        public String UserId {get; set;}
    }
}
