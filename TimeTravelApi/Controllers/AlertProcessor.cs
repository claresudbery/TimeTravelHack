using System;

namespace TimeTravelApi.Models
{
    public class AlertProcessor
    {
        public bool HasTimeRequestJustExpired(MoreTimeRequest timeRequest)
        {         
            bool expired = false;
            if (timeRequest.Expired == false)
            {
                var timeDifference = GetTimeDifferenceSinceRequest(timeRequest);

                if (timeDifference >= timeRequest.LengthInMinutes)
                {
                    expired = true;
                }
            }
            return expired;
        }

        public int GetTimeDifferenceSinceRequest(MoreTimeRequest timeRequest)
        {
            var timeDifference = DateTime.Now.TimeOfDay.Minutes - timeRequest.RequestTimeStamp.TimeOfDay.Minutes;
            if (timeDifference < 0)
            {
                timeDifference = timeDifference + 60;
            }
            return timeDifference;
        }
    }
}
