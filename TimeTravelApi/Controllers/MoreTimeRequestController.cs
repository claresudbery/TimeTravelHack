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
        private readonly ITimeTravelClock _clock;
        private readonly ITimeRequestData _context;

        public MoreTimeRequestController(ITimeRequestData timeRequestData, ITimeTravelClock clock)
        {
            _context = timeRequestData;
            _clock = clock;

            if (_context.NumTimeRequests() == 0)
            {
                // Create a new MoreTimeRequest if collection is empty,
                // which means you can't delete all MoreTimeRequests.
                _context.AddTimeRequest(
                    new MoreTimeRequest { 
                        RequestTimeStamp = _clock.Now,
                        Expired = true,
                        Alerted = true,
                        LengthInMinutes = 0
                    });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult<List<MoreTimeRequest>> GetAll()
        {
            return _context.AllTimeRequests();
        }

        [HttpGet("alert/{userId}", Name = "GetAlert")]
        public ActionResult<TimeAndAlert> GetAlert(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var alert = false;

            var timeRequestsReadyForAlert = _context
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
                    _context.UpdateTimeRequest(request);
                }
                _context.SaveChanges();
            }

            return new TimeAndAlert {Alert = alert};
        }

        [HttpGet("time/{userId}", Name = "GetTime")]
        public ActionResult<TimeAndAlert> GetTime(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var newTime = _clock.Now.AddMinutes(-_accumulatedTimeDifference);

            var justExpiredTimeRequests = _context
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
                    _context.UpdateTimeRequest(request);
                }
                _context.SaveChanges();
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
            _context.AddTimeRequest(newItem);
            _context.SaveChanges();

            return CreatedAtRoute(
                "GetAlert", 
                new { userId = newRequest.UserId }, 
                newItem);
        }
    }
}
