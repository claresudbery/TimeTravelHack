using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TimeTravelApi.Models;
using System;

namespace TimeTravelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoreTimeRequestController : ControllerBase
    {
        private static int _accumulatedTimeDifference = 0;
        private ITimeTravelClock _clock;
        private ITimeRequestData _timeRequestData;
        private readonly MoreTimeRequestContext _context;

        public MoreTimeRequestController(MoreTimeRequestContext context)
        {
            _context = context;
            _timeRequestData = new TimeRequestData(_context);
            _clock = new TimeTravelClock();
            if (_timeRequestData.NumTimeRequests() == 0)
            {
                // Create a new MoreTimeRequest if collection is empty,
                // which means you can't delete all MoreTimeRequests.
                _timeRequestData.AddTimeRequest(
                    new MoreTimeRequest { 
                        RequestTimeStamp = _clock.Now,
                        Expired = true,
                        Alerted = true,
                        LengthInMinutes = 0
                    });
                _timeRequestData.SaveChanges();
            }
        }

        public void InjectNewDependencies(ITimeRequestData timeRequestData, ITimeTravelClock clock)
        {
            _timeRequestData = timeRequestData;
            _clock = clock;
        }

        [HttpGet]
        public ActionResult<List<MoreTimeRequest>> GetAll()
        {
            return _timeRequestData.AllTimeRequests();
        }

        [HttpGet("alert/{userId}", Name = "GetAlert")]
        public ActionResult<TimeAndAlert> GetAlert(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var alert = false;

            var timeRequestsReadyForAlert = _timeRequestData
                .AllTimeRequests()
                .Where(x => x.UserId == userId)
                .Where(x => alertProcessor.IsTimeRequestReadyForAlert(x, _accumulatedTimeDifference, _clock))
                .ToList();

            if (timeRequestsReadyForAlert.Count() > 0)
            {
                alert = true;
                foreach (var request in timeRequestsReadyForAlert) 
                {
                    request.Alerted = true;
                    _timeRequestData.UpdateTimeRequest(request);
                }
                _timeRequestData.SaveChanges();
            }

            return new TimeAndAlert {Alert = alert};
        }

        [HttpGet("time/{userId}", Name = "GetTime")]
        public ActionResult<TimeAndAlert> GetTime(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var newTime = _clock.Now.AddMinutes(-_accumulatedTimeDifference);

            var justExpiredTimeRequests = _timeRequestData
                .AllTimeRequests()
                .Where(x => alertProcessor.HasTimeRequestJustExpired(x, _accumulatedTimeDifference, _clock))
                .ToList();
            
            if (justExpiredTimeRequests.Count() > 0)
            {
                var earliestExpiredRequestStartTime = justExpiredTimeRequests
                    .Select(x => x.RequestTimeStamp)
                    .Min();
                newTime = earliestExpiredRequestStartTime;

                var timeDifference = alertProcessor
                    .GetTimeDifferenceSinceRequest(earliestExpiredRequestStartTime, _accumulatedTimeDifference, _clock);

                _accumulatedTimeDifference += timeDifference;

                foreach (var request in justExpiredTimeRequests) 
                {
                    request.Expired = true;
                    _timeRequestData.UpdateTimeRequest(request);
                }
                _timeRequestData.SaveChanges();
            }

            return new TimeAndAlert {NewHours = newTime.TimeOfDay.Hours,
                                    NewMinutes = newTime.TimeOfDay.Minutes,
                                    NewSeconds = newTime.TimeOfDay.Seconds};
        }

        [HttpPost]
        public IActionResult Create(MoreTimeRequest newRequest)
        {
            var newItem = new MoreTimeRequest {
                RequestTimeStamp = _clock.Now.AddMinutes(-_accumulatedTimeDifference),
                Expired = false,
                Alerted = false,
                LengthInMinutes = newRequest.LengthInMinutes,
                UserId = newRequest.UserId
            };
            _timeRequestData.AddTimeRequest(newItem);
            _timeRequestData.SaveChanges();

            return CreatedAtRoute(
                "GetAlert", 
                new { userId = newRequest.UserId }, 
                newItem);
        }
    }
}
