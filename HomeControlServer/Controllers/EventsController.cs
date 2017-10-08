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
    //public class EventsController : ApiController
    //{
    //    public EventsController()
    //    {
    //    }

    //    public List<TimedEvent> Get()
    //    {
    //        return HeatingControl.events;
    //    }

    //    public HttpResponseMessage Post(TimedEvent timedEvent)
    //    {
    //        HeatingControl.AddEvent(timedEvent);

    //        var response = Request.CreateResponse<TimedEvent>(System.Net.HttpStatusCode.Created, timedEvent);

    //        return response;
    //    }

    //    //public HttpResponseMessage Put(TimedEvent timedEvent)
    //    //{
    //    //    //HeatingControl.updateEvent(timedEvent);

    //    //    //var response = Request.CreateResponse<TimedEvent>(System.Net.HttpStatusCode.OK, timedEvent);

    //    //    //return response;
    //    //}

    //    //public HttpResponseMessage Delete(int id)
    //    //{
    //    //    //HeatingControl.deleteEvent(id);

    //    //    //var response = Request.CreateResponse<TimedEvent>(System.Net.HttpStatusCode.OK, null);

    //    //    //return response;
    //    //}
    //}

    public class EventsController : ApiController
    {
        [HttpGet]
        [Route("api/events")]
        public IEnumerable<TimedEvent> GetAllEvents()
        {
            return HeatingControl.events;
        }

        [HttpGet]
        [Route("api/events/{id}")]
        public IHttpActionResult GetEvents(int id)
        {
            var timedEvent = HeatingControl.GetEventById(id);
            if (timedEvent == null)
            {
                return NotFound();
            }
            return Ok(timedEvent);
        }

        [HttpPost]
        [Route("api/events")]
        public IHttpActionResult PostEvents([FromBody] TimedEvent timedEvent)
        {
            timedEvent = HeatingControl.AddEvent(timedEvent);
            if (timedEvent == null)
            {
                return NotFound();
            }
            return Ok(timedEvent);
        }

        [HttpPut]
        [Route("api/events/{id}")]
        public IHttpActionResult PutEvents(int id, [FromBody] TimedEvent timedEvent)
        {
            var localEvent = HeatingControl.GetEventById(id);
            if (timedEvent == null)
            {
                return NotFound();
            }
            localEvent.setData(timedEvent);
            return Ok(timedEvent);
        }
    }
}
