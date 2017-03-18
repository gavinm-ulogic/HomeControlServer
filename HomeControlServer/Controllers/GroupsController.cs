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
    public class GroupsController : ApiController
    {
        public IEnumerable<EventGroup> GetAllEvents()
        {
            return HeatingControl.groups;
        }

        public IHttpActionResult GetGroups(int id)
        {
            var eventGroup = HeatingControl.GetGroupById(id);
            if (eventGroup == null)
            {
                return NotFound();
            }
            return Ok(eventGroup);
        }
    }
}

