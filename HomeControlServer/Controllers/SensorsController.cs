using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        //public IHttpActionResult GetSensor(int id)
        //{
        //    var sensor = HeatingControl.GetSensor(id);
        //    if (sensor == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(sensor);
        //}
    }
}
