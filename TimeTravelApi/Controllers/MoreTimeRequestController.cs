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
        private readonly MoreTimeRequestContext _context;

        public MoreTimeRequestController(MoreTimeRequestContext context)
        {
            _context = context;

            if (_context.MoreTimeRequests.Count() == 0)
            {
                // Create a new MoreTimeRequest if collection is empty,
                // which means you can't delete all MoreTimeRequests.
                _context.MoreTimeRequests.Add(
                    new MoreTimeRequest { 
                        RequestTimeStamp = DateTime.Now,
                        Expired = false,
                        LengthInMinutes = 20
                    });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult<List<MoreTimeRequest>> GetAll()
        {
            return _context.MoreTimeRequests.ToList();
        }

        [HttpGet("{id}", Name = "GetMoreTimeRequest")]
        public ActionResult<MoreTimeRequest> GetById(long id)
        {
            var item = _context.MoreTimeRequests.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        [HttpGet("{alert}", Name = "GetAlert")]
        public ActionResult<bool> GetAlert(bool hackyBool)
        {
            var alertProcessor = new AlertProcessor();
            var mostRecentTimeRequest = _context.MoreTimeRequests.Last();
            var alert = alertProcessor.HasTimeRequestExpired(mostRecentTimeRequest);

            if (alert)
            {
                mostRecentTimeRequest.Expired = true;
                _context.MoreTimeRequests.Update(mostRecentTimeRequest);
                _context.SaveChanges();
            }
            return alert;
        }

        [HttpPost]
        public IActionResult Create()
        {
            var newItem = new MoreTimeRequest {
                RequestTimeStamp = DateTime.Now,
                Expired = false,
                LengthInMinutes = 20
            };
            _context.MoreTimeRequests.Add(newItem);
            _context.SaveChanges();

            return CreatedAtRoute(
                "GetMoreTimeRequest", 
                new { id = newItem.Id }, 
                newItem);
        }
    }
}
