using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using HomeControlServer.Models;

namespace HomeControlServer.Providers
{
    public static class HeatingControl
    {
        private static int m_iNextEventId = 1;

        public class HeatingData
        {
            public List<EventGroup> theGroups = new List<EventGroup>();
            public List<Room> theRooms = new List<Room>();
            public List<Heater> theHeaters = new List<Heater>();
            public List<Sensor> theSensors = new List<Sensor>();
            public List<TimedEvent> theEvents = new List<TimedEvent>();
            public List<Relay> theRelays = new List<Relay>();
        }

        public static List<EventGroup> groups {get {return theData.theGroups;} set {} }
        public static List<Room> rooms { get { return theData.theRooms; } set { } }
        public static List<Heater> heaters { get { return theData.theHeaters; } set { } }
        public static List<Sensor> sensors { get { return theData.theSensors; } set { } }
        public static List<TimedEvent> events { get { return theData.theEvents; } set { } }
        public static List<Relay> relays { get { return theData.theRelays; } set { } }

        public static HeatingData theData;

        private static int highestEventId = 0;

        public static bool HolidayMode = false;
        public static bool FloorHeatActive = true;
        public static bool TowelRadsActive = true;
        
        public static List<Room> GetAllRooms()
        {
            return rooms;
        }

        public static bool Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(HeatingData));
            using (TextWriter writer = new StreamWriter(@"C:\Temp\HeatingData.xml"))
            {
                serializer.Serialize(writer, theData);
            }
            return true;
        }
        public static bool Load()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(HeatingData));
            TextReader reader = new StreamReader(@"C:\Temp\HeatingData.xml");

            Logger.Init();
            Logger.Log(Logger.LOGLEVEL_INFO, "###############################################################################################");
            Logger.Log(Logger.LOGLEVEL_INFO, "About to load init data");
            object obj = deserializer.Deserialize(reader);
            theData = (HeatingData)obj;
            reader.Close();
            Logger.Log(Logger.LOGLEVEL_INFO, "Init data loaded");

            // Add heaters to rooms
            foreach (Heater heater in heaters)
            {
                foreach (Room room in rooms)
                {
                    if (room.id == heater.roomId)
                    {
                        room.heaters.Add(heater);
                        break;
                    }
                }
            }

            // Add sensors to rooms and heaters
            foreach (Sensor sensor in sensors)
            {
                foreach (Room room in rooms)
                {
                    if (room.id == sensor.roomId)
                    {
                        room.sensors.Add(sensor);
                        foreach (Heater heater in room.heaters)
                        {// Add room sensor to heater
                            if (sensor.type == "room") { heater.sensors.Add(sensor); }   
                        }
                        break;
                    }
                }
            }

            foreach (TimedEvent timedEvent in events)
            {
                timedEvent.subjectType = "heater";
            }

            // Clear and then populate relay set
            Relay oRelay;
            theData.theRelays.Clear();
            foreach (Heater heater in heaters)
            {
                oRelay = new Relay(heater.name, heater.relayAddress);
                theData.theRelays.Add(oRelay);
            }

            theData.theRelays.Sort();

            Logger.Log(Logger.LOGLEVEL_INFO, "Init data processed");
            Save();
            return true;
        }

        public static Room GetRoom(int id)
        {
            Room result = rooms.Find(
            delegate(Room r)
            {
                return r.id == id;
            });

            return result;
        }

        public static EventGroup GetGroupById(int id)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].id == id) return groups[i];
            }
            return null;
        }

        public static Room GetRoomById(int id)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].id == id) return rooms[i];
            }
            return null;
        }

        public static Heater GetHeaterById(int id)
        {
            for (int i = 0; i < heaters.Count; i++)
            {
                if (heaters[i].id == id) return heaters[i];
            }
            return null;
        }

        public static TimedEvent GetEventById(int id)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].id == id) return events[i];
            }
            return null;
        }

        public static TimedEvent AddEvent(TimedEvent timedEvent)
        {
            if (timedEvent.id == 0)
            {
                highestEventId++;
                timedEvent.id = highestEventId;
            }
            else if (timedEvent.id > highestEventId) highestEventId = timedEvent.id;

            events.Add(timedEvent);
            return timedEvent;
        }

        public static TimedEvent UpdateEvent(TimedEvent timedEvent)
        {
            if (timedEvent.id != 0)
            {
                if (timedEvent.id > highestEventId) highestEventId = timedEvent.id;
                TimedEvent anEvent = GetEventById(timedEvent.id);
                if (anEvent != null) anEvent.setData(timedEvent);
                return timedEvent;
            }
            return null;
        }

        public static bool DeleteEvent(int timedEventId)
        {
            if (timedEventId != 0)
            {
                TimedEvent anEvent = GetEventById(timedEventId);
                if (anEvent != null) events.Remove(anEvent);
                return true;
            }
            return false;
        }

        public static List<TimedEvent> getHeaterEvents(int heaterId)
        {
            List<TimedEvent> retList = new List<TimedEvent>();
            foreach (TimedEvent timedEvent in events)
            {
                if (timedEvent.subjectId == heaterId)
                {
                    retList.Add(timedEvent);
                }
            }
            return retList;
        }
    }
}