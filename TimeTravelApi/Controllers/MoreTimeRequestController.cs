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
                _context.MoreTimeRequests.Add(new MoreTimeRequest { RequestTimeStamp = DateTime.Now });
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

        [HttpPost]
        public IActionResult Create(MoreTimeRequest item)
        {
            _context.MoreTimeRequests.Add(item);
            _context.SaveChanges();

            return CreatedAtRoute("GetMoreTimeRequest", new { id = item.Id }, item);
        }
    }
}
