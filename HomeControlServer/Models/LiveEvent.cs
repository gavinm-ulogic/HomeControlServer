using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeControlServer.Models
{
    public class LiveEvent
    {
        public TimedEvent timedEvent;
        public Heater heater;
        public Relay relay;
    }
}