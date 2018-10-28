using System;

namespace TimeTravelApi.Models
{
    public class AlertProcessor
    {
        public bool HasTimeRequestExpired(MoreTimeRequest timeRequest)
        {         
            bool expired = false;
            if (timeRequest.Expired == false)
            {
                var timeDifference = DateTime.Now.TimeOfDay.Minutes - timeRequest.RequestTimeStamp.TimeOfDay.Minutes;
                if (timeDifference < 0)
                {
                    timeDifference = timeDifference + 60;
                }

                if (timeDifference >= 1)
                // TODO: change to 20 minutes instead of 1
                {
                    expired = true;
                }
            }
            return expired;
        }
    }
}
