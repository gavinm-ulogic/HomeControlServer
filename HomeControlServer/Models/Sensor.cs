using System;
using System.Xml.Serialization;
using HomeControlServer.Providers;

namespace HomeControlServer.Models
{

    public class Sensor
    {
        public const int NO_READING = -999;
        public const int MAX_VALID = 80;
        public const int MIN_VALID = -40;
        //public const int SENSOR_TYPE_ROOM = 1;
        //public const int SENSOR_TYPE_FLOOR = 2;

        private int m_iReading = NO_READING;

        public int id { get; set; }
        public string name { get; set; }
        public int roomId { get; set; }
        public string type { get; set; }        // room, floor
        public string sensorId { get; set; }
        public string maxValue { get; set; }
        public System.DateTime lastChange { get; set; }
        public System.DateTime lastRead { get; set; }

        public Sensor()
        {
            this.id = 0;
            this.type = "";
            this.name = "";
            this.roomId = 0;
            this.sensorId = "";
            this.maxValue = "";
        }

        public Sensor(int id, string type, string name, int room, string owid, string maxValue)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.roomId = room;
            this.sensorId = owid;
            this.maxValue = maxValue;
        }

        public int reading
        {
            get { return m_iReading; }
            set
            {
                var lastReading = m_iReading;
                if ((m_iReading != value) && IsValid(value))
                {
                    m_iReading = value;
                    lastChange = DateTime.Now;
                }
                lastRead = DateTime.Now;
                Logger.Log(Logger.LOGLEVEL_INFO, name + " previous: " + lastReading + " new: " + 
                    m_iReading + " read: " + lastRead.ToString("yyyyMMdd H:mm") + " changed: " + lastChange.ToString("yyyyMMdd H:mm"));
            }
        }

        private bool IsValid(int p_Reading)
        {
            return ((p_Reading > MIN_VALID) && (p_Reading < MAX_VALID));
        }

        [XmlIgnoreAttribute]
        public string lastChangeStr
        {
            get { return lastChange.ToString(Globals.DATETIME_FORMAT); }
            set { ;}
        }

        [XmlIgnoreAttribute]
        public string lastReadStr
        {
            get { return lastRead.ToString(Globals.DATETIME_FORMAT); }
            set { ;}
        }
    }
}

