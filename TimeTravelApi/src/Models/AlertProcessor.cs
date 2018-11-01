using System;

namespace TimeTravelApi.Models
{
    public class AlertProcessor
    {
        public bool IsTimeRequestReadyForAlert(
            MoreTimeRequest timeRequest, 
            int accumulatedTime,
            ITimeTravelClock clock)
        {         
            bool alert = false;
            if (timeRequest.Alerted == false)
            {
                var timeDifference = GetTimeDifferenceSinceRequest(
                    timeRequest.RequestTimeStamp, 
                    accumulatedTime,
                    clock);

                if (timeDifference >= timeRequest.LengthInMinutes)
                {
                    alert = true;
                }
            }
            return alert;
        }

        public bool HasTimeRequestJustExpired(
            MoreTimeRequest timeRequest, 
            int accumulatedTime,
            ITimeTravelClock clock)
        {         
            bool expired = false;
            if (timeRequest.Expired == false)
            {
                var timeDifference = GetTimeDifferenceSinceRequest(
                    timeRequest.RequestTimeStamp, 
                    accumulatedTime,
                    clock);

                if (timeDifference >= timeRequest.LengthInMinutes)
                {
                    expired = true;
                }
            }
            return expired;
        }

        public int GetTimeDifferenceSinceRequest(
            DateTime requestTimestamp, 
            int accumulatedTime,
            ITimeTravelClock clock)
        {
            var timeNow = clock.Now;
            var timeDifference = timeNow.AddMinutes(-accumulatedTime)
                                         .TimeOfDay.Minutes - requestTimestamp.TimeOfDay.Minutes;
            if (timeDifference < 0)
            {
                timeDifference = timeDifference + 60;
            }
            return timeDifference;
        }
    }
}
