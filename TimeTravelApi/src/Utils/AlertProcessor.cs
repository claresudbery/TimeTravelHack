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

        private DateTime RemoveSeconds(DateTime source)
        {
            return new DateTime(
                source.Year,
                source.Month,
                source.Day,
                source.Hour,
                source.Minute,
                0
            );
        }

        public int GetTimeDifferenceSinceRequest(
            MoreTimeRequest request, 
            int accumulatedTime,
            ITimeTravelClock clock)
        {
            var negativeTimeDifference = request.TimeAdjustmentAtCreationTime * -1;
            var timeNowFromRequestUserPerspective = RemoveSeconds(clock.Now.AddMinutes(negativeTimeDifference));
            var requestTimeWithoutSeconds = RemoveSeconds(request.RequestTimeStamp);
            var timeDifference = Convert.ToInt32(
                timeNowFromRequestUserPerspective
                    .Subtract(requestTimeWithoutSeconds)
                    .TotalMinutes);
            return timeDifference;
        }
    }
}
