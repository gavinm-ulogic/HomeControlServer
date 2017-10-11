using System.Collections.Generic;

namespace HomeControlServer.Models
{
    public class EventGroup
    {
        public int id { get; set; }
        public string name { get; set; }

        public List<TimedEvent> timedEvents = new List<TimedEvent>();

        public EventGroup()
        {
            this.id = 0;
            this.name = "";
        }

        public EventGroup(int id, string name) 
        {
            this.id = id;
            this.name = name;
        }
    }
}