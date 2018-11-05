using System;
using TimeTravelApi.Models;

namespace TimeTravelApi.Utils
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
                    timeRequest, 
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
                    timeRequest, 
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
            MoreTimeRequest request, 
            int accumulatedTime,
            ITimeTravelClock clock)
        {
            var negativeTimeDifference = request.TimeAdjustmentAtCreationTime * -1;
            var timeNowFromRequestUserPerspective = clock.Now.AddMinutes(negativeTimeDifference);
            var timeDifference = Convert.ToInt32(
                timeNowFromRequestUserPerspective
                    .Subtract(request.RequestTimeStamp)
                    .TotalMinutes);
            return timeDifference;
        }
    }
}
