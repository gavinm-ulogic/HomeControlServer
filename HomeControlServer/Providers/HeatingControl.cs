using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using HomeControlServer.Models;
using System;
using Newtonsoft.Json;
using System.Timers;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace HomeControlServer.Providers
{

    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Room) && (property.PropertyName == "heaters" || property.PropertyName == "sensors"))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        return false;
                    };
            }

            return property;
        }
    }

    public static class HeatingControl
    {
        private const string DATADIR = @"C:\HomeControl\Data\";
        private const string CONTROLFILE = DATADIR + @"HeatingData.json";
        private const string CONTROLFILEALT = DATADIR + @"HeatingDataAlt.json";
        private const string CONTROLFILEFALLBACK = DATADIR + @"HeatingDataFallback.json";

        public class HeatingData
        {
            public List<EventGroup> groups = new List<EventGroup>();
            public List<Room> rooms = new List<Room>();
            public List<Heater> heaters = new List<Heater>();
            public List<Sensor> sensors = new List<Sensor>();
            public List<TimedEvent> events = new List<TimedEvent>();
            public List<Relay> relays = new List<Relay>();
        }

        public static HeatingData heatingData;

        private static int highestEventId = 0;

        private static Timer altTimer = null;

        //public static bool holidayMode = false;
        //public static bool floorHeatActive = true;
        //public static bool towelRadsActive = true;

        public static DateTime? lastAltSave = null;

        public static bool Save()
        {
            try
            {
                string saveJson = JsonConvert.SerializeObject(heatingData, Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
                if (saveJson == null || saveJson == "") throw new Exception("Json conversion error");
                using (TextWriter writer = new StreamWriter(CONTROLFILE))
                {
                    writer.Write(saveJson);
                    writer.Close();
                }

                if (altTimer == null)
                {
                    altTimer = new Timer(5000);
                    altTimer.AutoReset = false;
                    altTimer.Elapsed += SaveAlt;
                }

                if (!altTimer.Enabled)
                {
                    altTimer.Enabled = true;
                    altTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LOGLEVEL_ERROR, $"Failed to save data file - {CONTROLFILE} - {ex.Message}");
            }
            return true;
        }

        private static void SaveAlt(Object source, ElapsedEventArgs e)
        {
            altTimer.Enabled = false;
            try
            {
                string saveJson = JsonConvert.SerializeObject(heatingData, Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
                if (saveJson == null || saveJson == "") throw new Exception("Json conversion error");
                using (TextWriter writer = new StreamWriter(CONTROLFILEALT))
                {
                    writer.Write(saveJson);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LOGLEVEL_ERROR, $"Failed to save data file - {CONTROLFILEALT} - {ex.Message}");
            }
        }

        public static bool Load()
        {
            Logger.Init();
            Logger.Log(Logger.LOGLEVEL_INFO, "###############################################################################################");
            Logger.Log(Logger.LOGLEVEL_INFO, "About to load init data");

            try
            {
                using (TextReader reader = new StreamReader(CONTROLFILE))
                {
                    heatingData = JsonConvert.DeserializeObject<HeatingData>(reader.ReadToEnd());
                    reader.Close();
                }
            }
            catch
            {
                try
                {
                    Logger.Log(Logger.LOGLEVEL_ERROR, "Failed to load data file - trying alt file");
                    using (TextReader reader = new StreamReader(CONTROLFILEALT))
                    {
                        heatingData = JsonConvert.DeserializeObject<HeatingData>(reader.ReadToEnd());
                        reader.Close();
                    }
                }
                catch
                {
                    Logger.Log(Logger.LOGLEVEL_ERROR, "Failed to load alt data file - trying fallback");
                    using (TextReader reader = new StreamReader(CONTROLFILEFALLBACK))
                    {
                        heatingData = JsonConvert.DeserializeObject<HeatingData>(reader.ReadToEnd());
                        reader.Close();
                    }

                }
            }

            Logger.Log(Logger.LOGLEVEL_INFO, "Init data loaded");

            if (heatingData == null)
            {
                return false;
            }

            heatingData.rooms.ForEach(r =>
            {
                var roomHeaters = heatingData.heaters.FindAll(h => h.roomId == r.id);
                var roomSensors = heatingData.sensors.FindAll(s => s.roomId == r.id);
                r.sensors = roomSensors;
                roomHeaters.ForEach(rh => rh.sensors = roomSensors);
                r.heaters = roomHeaters;
            });

            // Clear and then populate relay set
            heatingData.relays.Clear();
            heatingData.heaters.ForEach(h =>
            {
                var heaterRelay = new Relay(h.name, h.relayAddress);
                heatingData.relays.Add(heaterRelay);

            });

            heatingData.relays.Sort();

            Logger.Log(Logger.LOGLEVEL_INFO, "Init data processed");
            Save();
            return true;
        }

        public static List<Room> GetAllRooms()
        {
            return heatingData.rooms;
        }

        public static Room GetRoom(int id)
        {
            return heatingData.rooms.Find(x => x.id == id);
        }

        public static List<Sensor> GetAllSensors()
        {
            return heatingData.sensors;
        }

        public static Sensor GetSensor(int id)
        {
            return heatingData.sensors.Find(x => x.id == id);
        }

        public static List<Heater> GetAllHeaters()
        {
            return heatingData.heaters;
        }

        public static Heater GetHeater(int id)
        {
            return heatingData.heaters.Find(x => x.id == id);
        }

        public static List<Relay> GetAllRelays()
        {
            return heatingData.relays;
        }

        public static Relay GetRelayByAddress(string relayAddress)
        {
            return heatingData.relays.Find(x => x.address == relayAddress);
        }

        public static List<TimedEvent> GetAllEvents()
        {
            return heatingData.events;
        }

        public static TimedEvent GetEvent(int id)
        {
            return heatingData.events.Find(x => x.id == id);
        }

        public static TimedEvent AddEvent(TimedEvent timedEvent)
        {
            if (timedEvent.id == 0)
            {
                highestEventId++;
                timedEvent.id = highestEventId;
            }
            else if (timedEvent.id > highestEventId) highestEventId = timedEvent.id;

            heatingData.events.Add(timedEvent);
            return timedEvent;
        }

        public static TimedEvent UpdateEvent(TimedEvent timedEvent)
        {
            if (timedEvent.id != 0)
            {
                if (timedEvent.id > highestEventId) highestEventId = timedEvent.id;
                TimedEvent anEvent = GetEvent(timedEvent.id);
                if (anEvent != null) anEvent.setData(timedEvent);
                return timedEvent;
            }
            return null;
        }

        public static bool DeleteEvent(int timedEventId)
        {
            if (timedEventId != 0)
            {
                TimedEvent anEvent = GetEvent(timedEventId);
                if (anEvent != null) heatingData.events.Remove(anEvent);
                return true;
            }
            return false;
        }

        public static Status GetStatus()
        {
            var status = new Status();
            
            foreach(TimedEvent te in heatingData.events)
            {
                if (te.IsActive(DateTime.MinValue))
                {
                    var le = new LiveEvent();
                    le.timedEvent = te;
                    le.heater = HeatingControl.GetHeater(te.subjectId);
                    le.relay = HeatingControl.GetRelayByAddress(le.heater.relayAddress);
                    status.liveEvents.Add(le);
                }
                else if (te.IsActive(DateTime.MinValue, Status.SOONTIME))
                {
                    var le = new LiveEvent();
                    le.timedEvent = te;
                    le.heater = HeatingControl.GetHeater(te.subjectId);
                    le.relay = HeatingControl.GetRelayByAddress(le.heater.relayAddress);
                    status.soonEvents.Add(le);
                }
            }

            status.sensors = heatingData.sensors;

            return status;
        }



        //public static List<TimedEvent> getHeaterEvents(int heaterId)
        //{
        //    List<TimedEvent> retList = new List<TimedEvent>();
        //    foreach (TimedEvent timedEvent in events)
        //    {
        //        if (timedEvent.subjectId == heaterId)
        //        {
        //            retList.Add(timedEvent);
        //        }
        //    }
        //    return retList;
        //}
    }
}