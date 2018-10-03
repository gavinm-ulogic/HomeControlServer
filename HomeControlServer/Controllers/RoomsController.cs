using System.Collections.Generic;
using System.Web.Http;
using HomeControlServer.Models;
using HomeControlServer.Providers;

namespace HomeControlServer.Controllers
{
    public class RoomsController : ApiController
    {
        [HttpOptions]
        [Route("api/rooms/{id}")]
        public void Options2() { }

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

        [Route("api/rooms/{id}")]
        [HttpPut]
        public IHttpActionResult UpdateRoom([FromBody] Room room)
        {
            var r = HeatingControl.GetRoom(room.id);
            if (r != null)
            {
                r.name = room.name;
                r.tempTarget = room.tempTarget;
                r.tempMin = room.tempMin;
                r.tempMax = room.tempMax;
            }
            //HeatingControl.Save();
            return Ok(room);
        }

    }
}
