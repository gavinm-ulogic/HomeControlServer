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
    public class RoomsController : ApiController
    {
        public IEnumerable<Room> GetAllRooms()
        {
            return HeatingControl.rooms;
        }

        public IHttpActionResult GetRooms(int id)
        {
            var room = HeatingControl.GetRoom(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }
    }
}
