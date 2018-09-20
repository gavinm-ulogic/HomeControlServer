using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeControlServer.Models
{
    public class Status
    {
        public const int SOONTIME = 3600;

        public List<LiveEvent> liveEvents { get; set; }
        public List<LiveEvent> soonEvents { get; set; }
        public List<Sensor> sensors { get; set; }

        public Status()
        {
            this.liveEvents = new List<LiveEvent>();
            this.soonEvents = new List<LiveEvent>();
            this.sensors = new List<Sensor>();
        }
    }
}