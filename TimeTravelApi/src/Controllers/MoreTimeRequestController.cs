using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TimeTravelApi.Models;
using System;
using TimeTravelApi.Utils;

namespace TimeTravelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoreTimeRequestController : ControllerBase
    {
        private readonly ITimeTravelClock _clock;
        private readonly ITimeRequestData _timeRequestData;
        private readonly ITimeTracker _timeTracker;
        private readonly MoreTimeRequestContext _dbContext;

        public MoreTimeRequestController(
            MoreTimeRequestContext context,
            ITimeRequestData timeRequestData,
            ITimeTravelClock timeTravelClock,
            ITimeTracker timeTracker)
        {
            _dbContext = context;
            _timeRequestData = timeRequestData;
            _clock = timeTravelClock;
            _timeTracker = timeTracker;
            if (_timeRequestData.NumTimeRequests(_dbContext) == 0)
            {
                // Create a new MoreTimeRequest if collection is empty,
                // which means you can't delete all MoreTimeRequests.
                _timeRequestData.AddTimeRequest(
                    _dbContext,
                    new MoreTimeRequest { 
                        RequestTimeStamp = _clock.Now,
                        Expired = true,
                        Alerted = true,
                        LengthInMinutes = 0
                    });
                _timeRequestData.SaveChanges(_dbContext);
            }
        }

        [HttpGet]
        public ActionResult<List<MoreTimeRequest>> GetAll()
        {
            return _timeRequestData.AllTimeRequests(_dbContext);
        }

        [HttpGet("alert/{userId}", Name = "GetAlert")]
        public ActionResult<TimeAndAlert> GetAlert(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var alert = false;

            var timeRequestsReadyForAlert = _timeRequestData
                .AllTimeRequests(_dbContext)
                .Where(x => x.UserId == userId)
                .Where(x => alertProcessor.IsTimeRequestReadyForAlert(x, _timeTracker.AccumulatedTimeDifference, _clock))
                .ToList();

            if (timeRequestsReadyForAlert.Count() > 0)
            {
                alert = true;
                foreach (var request in timeRequestsReadyForAlert) 
                {
                    request.Alerted = true;
                    _timeRequestData.UpdateTimeRequest(_dbContext, request);
                }
                _timeRequestData.SaveChanges(_dbContext);
            }

            return new TimeAndAlert {Alert = alert};
        }

        [HttpGet("time/{userId}", Name = "GetTime")]
        public ActionResult<TimeAndAlert> GetTime(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var newTime = _clock.Now.AddMinutes(-_timeTracker.AccumulatedTimeDifference);

            var justExpiredTimeRequests = _timeRequestData
                .AllTimeRequests(_dbContext)
                .Where(x => alertProcessor.HasTimeRequestJustExpired(x, _timeTracker.AccumulatedTimeDifference, _clock))
                .ToList();
            
            if (justExpiredTimeRequests.Count() > 0)
            {
                var earliestExpiredRequestStartTime = justExpiredTimeRequests
                    .Select(x => x.RequestTimeStamp)
                    .Min();
                newTime = earliestExpiredRequestStartTime;

                var timeDifference = alertProcessor
                    .GetTimeDifferenceSinceRequest(earliestExpiredRequestStartTime, _timeTracker.AccumulatedTimeDifference, _clock);

                _timeTracker.AccumulatedTimeDifference += timeDifference;

                foreach (var request in justExpiredTimeRequests) 
                {
                    request.Expired = true;
                    _timeRequestData.UpdateTimeRequest(_dbContext, request);
                }
                _timeRequestData.SaveChanges(_dbContext);
            }

            return new TimeAndAlert {NewHours = newTime.TimeOfDay.Hours,
                                    NewMinutes = newTime.TimeOfDay.Minutes,
                                    NewSeconds = newTime.TimeOfDay.Seconds};
        }

        [HttpPost]
        public IActionResult Create(MoreTimeRequest newRequest)
        {
            var newItem = new MoreTimeRequest {
                RequestTimeStamp = _clock.Now.AddMinutes(-_timeTracker.AccumulatedTimeDifference),
                Expired = false,
                Alerted = false,
                LengthInMinutes = newRequest.LengthInMinutes,
                UserId = newRequest.UserId
            };
            _timeRequestData.AddTimeRequest(_dbContext, newItem);
            _timeRequestData.SaveChanges(_dbContext);

            return CreatedAtRoute(
                "GetAlert", 
                new { userId = newRequest.UserId }, 
                newItem);
        }
    }
}
