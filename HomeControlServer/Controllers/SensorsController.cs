using System.Collections.Generic;
using System.Web.Http;
using HomeControlServer.Providers;
using HomeControlServer.Models;

namespace HomeControlServer.Controllers
{
    public class SensorsController : ApiController
    {
        public IEnumerable<Sensor> GetAllSensors()
        {
            return HeatingControl.sensors;
        }

        [HttpOptions]
        [Route("api/sensors/{id}")]
        public void Options2() { }

        [Route("api/sensors/{id}")]
        [HttpPut]
        public IHttpActionResult UpdateSensor([FromBody] Sensor sensor)
        {
            var s = HeatingControl.GetSensor(sensor.id);
            if (s != null)
            {
                s.name = sensor.name;
            }
            //HeatingControl.Save();
            return Ok(sensor);
        }
    }
}
