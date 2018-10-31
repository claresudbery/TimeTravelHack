using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTravelApi.Models
{
    public class TimeTravelClock : ITimeTravelClock
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}
