using System;

namespace TimeTravelApi.Models
{
    public class TimeAndAlert
    {   
        public int NewHours { get; set;}
        public int NewMinutes { get; set; }
        public int NewSeconds { get; set; }
        public bool Alert {get; set;}
    }
}
