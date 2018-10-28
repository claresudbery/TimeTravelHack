using System;

namespace TimeTravelApi.Models
{
    public class AlertProcessor
    {
        public bool HasTimeRequestJustExpired(MoreTimeRequest timeRequest, int accumulatedTime)
        {         
            bool expired = false;
            if (timeRequest.Expired == false)
            {
                var timeDifference = GetTimeDifferenceSinceRequest(timeRequest.RequestTimeStamp, accumulatedTime);

                if (timeDifference >= timeRequest.LengthInMinutes)
                {
                    expired = true;
                }
            }
            return expired;
        }

        public int GetTimeDifferenceSinceRequest(DateTime requestTimestamp, int accumulatedTime)
        {
            var timeDifference = DateTime.Now.AddMinutes(-accumulatedTime)
                                         .TimeOfDay.Minutes - requestTimestamp.TimeOfDay.Minutes;
            if (timeDifference < 0)
            {
                timeDifference = timeDifference + 60;
            }
            return timeDifference;
        }
    }
}
