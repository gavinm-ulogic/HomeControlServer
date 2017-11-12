using System.Collections.Generic;
using System.Web.Http;
using HomeControlServer.Models;
using HomeControlServer.Providers;

namespace HomeControlServer.Controllers
{
    public class EventsController : ApiController
    {
        [HttpOptions]
        [Route("api/events")]
        public void Options1() { }

        [HttpOptions]
        [Route("api/events/{id}")]
        public void Options2() { }

        [HttpGet]
        [Route("api/events")]
        public IEnumerable<TimedEvent> GetAllEvents()
        {
            return HeatingControl.events;
        }

        [HttpPost]
        [Route("api/events")]
        public IHttpActionResult PostEvents([FromBody] TimedEvent timedEvent)
        {
            if (timedEvent.id > 0)
            {
                return PutEvents(timedEvent.id, timedEvent);
            }
            else
            {
                timedEvent = HeatingControl.AddEvent(timedEvent);
                if (timedEvent == null)
                {
                    return NotFound();
                }
                return Ok(timedEvent);
            }
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

        [HttpDelete]
        [Route("api/events/{id}")]
        public IHttpActionResult DeleteEvent(int id)
        {
            HeatingControl.DeleteEvent(id);
            return Ok();
        }
    }
}
