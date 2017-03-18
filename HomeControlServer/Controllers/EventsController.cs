using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HomeControlServer.Models;
using HomeControlServer.Providers;

namespace HomeControlServer.Controllers
{
    public class EventsController : ApiController
    {
        public IEnumerable<TimedEvent> GetAllEvents()
        {
            return HeatingControl.events;
        }

        public IHttpActionResult GetEvents(int id)  
        {
            var timedEvent = HeatingControl.GetEventById(id);
            if (timedEvent == null)
            {
                return NotFound();
            }
            return Ok(timedEvent);
        }
    }
}
