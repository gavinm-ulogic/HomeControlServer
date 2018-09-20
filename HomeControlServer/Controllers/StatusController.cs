using System.Collections.Generic;
using System.Web.Http;
using HomeControlServer.Models;
using HomeControlServer.Providers;

namespace HomeControlServer.Controllers
{
    public class StatusController : ApiController
    {
        public IHttpActionResult GetStatus()
        {
            var status = HeatingControl.GetStatus();
            return Ok(status);
        }
    }
}
