using System.Collections.Generic;
using System.Web.Http;
using HomeControlServer.Providers;
using HomeControlServer.Models;

namespace HomeControlServer.Controllers
{
    public class HeatersController : ApiController
    {
        public IEnumerable<Heater> GetAllHeaters()
        {
            return HeatingControl.GetAllHeaters();
        }

        [HttpOptions]
        [Route("api/heaters/{id}")]
        public void Options2() { }

        [Route("api/heaters/{id}")]
        [HttpPut]
        public IHttpActionResult UpdateHeater([FromBody] Heater heater)
        {
            var h = HeatingControl.GetHeater(heater.id);
            if (h != null)
            {
                h.name = heater.name;
            }
            //HeatingControl.Save();
            return Ok(heater);
        }
    }
}
