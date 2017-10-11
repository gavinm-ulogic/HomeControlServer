using System.Collections.Generic;
using HomeControlServer.Providers;
using System.Xml.Serialization;

namespace HomeControlServer.Models
{

    public class Heater
    {
        [XmlIgnoreAttribute]
        public List<Sensor> sensors = new List<Sensor>();

        public int id { get; set; }
        public string type { get; set; }        // floor, towelrad
        public string name { get; set; }
        public int roomId { get; set; }
        public string relayAddress { get; set; }
        public int tempMin { get; set; }
        public int tempMax { get; set; }
        public int tempTarget { get; set; }

        public Heater()
        {
            this.id = 0;
            this.type = "";
            this.name = "";
            this.roomId = 0;
            this.relayAddress = "";
            this.tempMin = 0;
            this.tempMax = 0;
            this.tempTarget = 0;
        }

        public Heater(int id, string type, string name, int room, string relay, int tempMin, int tempMax, int tempTarget)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.roomId = room;
            this.relayAddress = relay;
            this.tempMin = tempMin;
            this.tempMax = tempMax;
            this.tempTarget = tempTarget;
        }

        public int state
        {
            get { return (RelayControl.GetRelayState(relayAddress)) ? 1 : 0; }
            set { ;}
        }
    }
}

