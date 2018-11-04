using System;
using System.Collections.Generic;
using System.Text;
using TimeTravelApi.Models;
using TimeTravelApi.Tests.TestUtils;

namespace TimeTravelApi.Tests.ModelBuilders
{
    class TimeRequestModelBuilder
    {
        private MoreTimeRequest _timeRequest;

        public TimeRequestModelBuilder()
        {
            _timeRequest = new MoreTimeRequest()
            {
                Alerted = false,
                Expired = false,
                LengthInMinutes = TimeConstants.DefaultRequestLengthInMinutes,
                MinutesToAdjustClockBy = TimeConstants.DefaultRequestLengthInMinutes,
                RequestTimeStamp = DateTime.Now,
                UserId = "Some user"
            };
        }

        public MoreTimeRequest Model()
        {
            return _timeRequest;
        }

        public TimeRequestModelBuilder WithAlerted(bool alerted)
        {
            _timeRequest.Alerted = alerted;
            return this;
        }

        public TimeRequestModelBuilder WithExpired(bool expired)
        {
            _timeRequest.Expired = expired;
            return this;
        }

        public TimeRequestModelBuilder WithLengthInMinutes(int length)
        {
            _timeRequest.LengthInMinutes = length;
            return this;
        }

        public TimeRequestModelBuilder WithMinutesToAdjustClockBy(int minutes)
        {
            _timeRequest.MinutesToAdjustClockBy = minutes;
            return this;
        }

        public TimeRequestModelBuilder WithRequestTimeStamp(DateTime timeStamp)
        {
            _timeRequest.RequestTimeStamp = timeStamp;
            return this;
        }

        public TimeRequestModelBuilder WithUserId(string userId)
        {
            _timeRequest.UserId = userId;
            return this;
        }
    }
}
