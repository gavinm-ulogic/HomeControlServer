using System.Collections.Generic;
using System.Xml.Serialization;

namespace HomeControlServer.Models
{
    public class Room
    {
        public int id { get; set; }
        public string name { get; set; }
        public int groupId { get; set; }
        public int tempMin { get; set; }
        public int tempMax { get; set; }
        public int tempTarget { get; set; }

        [XmlIgnoreAttribute]
        public List<Sensor> sensors = new List<Sensor>();
        [XmlIgnoreAttribute]
        public List<Heater> heaters = new List<Heater>();
        //public List<TimedEvent> TimedEvents = new List<TimedEvent>();

        public Room()
        {
            this.id = 0;
            this.name = "";
            this.groupId = 0;
            this.tempMin = 0;
            this.tempMax = 0;
            this.tempTarget = 0;
        }

        public Room(int id, string name, int group, int tempMin, int tempMax, int tempTarget) 
        {
            this.id = id;
            this.name = name;
            this.groupId = 0;
            this.tempMin = tempMin;
            this.tempMax = tempMax;
            this.tempTarget = tempTarget;
        }

        public int tempCurrent
        {
            get
            {
                int TempSum = 0;
                int TempCount = 0;
                for (int i = 0; i < sensors.Count; i++)
                {
                    if ((sensors[i].type == "room")&&(sensors[i].reading != Sensor.NO_READING))
                    {
                        TempSum += sensors[i].reading;
                        TempCount++;
                    }
                }
                return (TempCount == 0) ? Sensor.NO_READING : TempSum / TempCount;
            }
            set { ;}
        }

    }
}