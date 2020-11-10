using System;
using System.Collections.ObjectModel;
using HomeControlServer.Providers;

namespace HomeControlServer.Models
{
    public class TimedEventPeriod
    {
        private uint days;
        private uint time;
        private uint duration;
        private string absoluteDate;

        public string value {
            get { return $"{days}-{time}-{duration}"; }
        }

        public TimedEventPeriod()
        {
        }

        public TimedEventPeriod(string tep)
        {
            // format:  dayencode-secsincemidnight-durationinsecs
            //          where dayencode is either 7 bit encode day of week 1=Mon, 2=Tue, 4=Wed ... 3=Mon & Tue, weekdays & weekend can be inferred
            //          or dayencode is actual date e.g. 20201107 - 7th November 2020

            var splitTep = tep.Split('-');
            if (splitTep.Length != 3)
            {
                throw new Exception("Incorrect event period format");
            }

            var dateEncoded = UInt32.Parse(splitTep[0]);
            if (dateEncoded > 127)
            { // absolute date
                throw new NotImplementedException();
            }
            else
            {
                days = dateEncoded;
            }

            time = UInt32.Parse(splitTep[1]);
            duration = UInt32.Parse(splitTep[2]);
        }


        public string description
        {
            get
            {
                string retStr = "";
                string daysStr = "";
                string timeStr = "";
                string periodStr = "";
                if (absoluteDate != null)
                {
                    throw new NotImplementedException();
                }
                else
                {// recuring event
                    switch (days)
                    {
                        case 31:
                            daysStr = "Week days";
                            break;
                        case 96:
                            daysStr = "Weekend";
                            break;
                        case 127:
                            daysStr = "All days";
                            break;
                        default:
                            daysStr += ((days & 1) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Mon" : "");
                            daysStr += ((days & 2) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Tue" : "");
                            daysStr += ((days & 4) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Wed" : "");
                            daysStr += ((days & 8) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Thu" : "");
                            daysStr += ((days & 16) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Fri" : "");
                            daysStr += ((days & 32) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Sat" : "");
                            daysStr += ((days & 64) > 0 ? (daysStr.Length > 0 ? ", " : "") + "Sun" : "");
                            break;
                        }
                    if (daysStr == "") daysStr = "No days";
                    retStr += daysStr + " ";
                }

                var hours = time / 3600;
                var mins = (time - hours * 3600) / 60;
                var secs = time - hours * 3600 - mins * 60;

                timeStr = $"{((uint)hours).ToString("D2")}:{((uint)mins).ToString("D2")}";
                timeStr += secs > 0 ? $":{((uint)secs).ToString("D2")}" : "";

                var periodMins = Math.Floor((decimal)duration / 60);
                var periodSecs = duration - periodMins * 60;

                periodStr = $"{((uint)periodMins).ToString()} mins";
                periodStr += periodSecs > 0 ? $" {((uint)periodSecs).ToString()} secs" : "";

                retStr += timeStr + " for " + periodStr;

                return retStr;
            }
            set {; }
        }

        public bool isActive(DateTime refTime, int offsetSecs = 0)
        {
            if (refTime == null || refTime == DateTime.MinValue)
            {
                refTime = DateTime.Now;
            }
            if (offsetSecs > 0) refTime = refTime.AddSeconds(offsetSecs);

            if (absoluteDate != null)
            {
                throw new NotImplementedException();
            }
            else
            { // Not a fully specified range - i.e. repeating event
                var refDay = (int)refTime.DayOfWeek;
                refDay--; // mon = 0;
                if (refDay < 0) refDay = 6; // move sunday to after saturday
                var refDayBin = (uint)Math.Pow(2, refDay);
                var refTimeOfDay = refTime.TimeOfDay.Ticks / 10000000;

                var nextDayPeriod = time + duration > 60 * 60 * 24 ? time + duration - 60 * 60 * 24 : 0; // will be > 0 for period that spans midnight

                if (nextDayPeriod > 0)
                { // need to check end of prev day & start of this day
                    int prevDay = refDay - 1;
                    if (prevDay < 0) prevDay = 0;
                    int prevDayBin = (int)Math.Pow(2, prevDay);

                    if ((prevDayBin & days) > 0 && refTimeOfDay >= time) return true; // between start prev day & midnight
                    if ((refDayBin & days) > 0 && refTimeOfDay < nextDayPeriod) return true; // between start of day & end of period
                }
                else
                {
                    return (refDayBin & days) > 0 && refTimeOfDay >= time && refTimeOfDay < time + duration;
                }
            }

            return false;
        }

        public bool isPast(DateTime refTime)
        {
            if (refTime == System.DateTime.MinValue)
            {
                refTime = System.DateTime.Now;
            }

            if (absoluteDate != null)
            { // Fully specified datetime range
                var startDate = new DateTime(Int32.Parse(absoluteDate.Substring(0, 4)), Int32.Parse(absoluteDate.Substring(4, 2)), Int32.Parse(absoluteDate.Substring(6, 2)));
                var startTime = startDate.AddSeconds(time);
                var endTime = startTime.AddSeconds(duration);
                if (refTime < endTime)
                {
                    return false;
                }
            }
            else
            {
                // Repeating event 
                return false;
            }
            return true;
        }



    }


    public class TimedEvent
    {
        private int m_iId = 0;
        private string m_sSubjectType = "";
        private int m_iSubjectId = 0;
        //private System.DateTime m_dTimeStart;
        //private System.DateTime m_dTimeEnd;
        private string m_sAction = "off";       // off, timer (ignores sensors), target (uses sensors)
        private TimedEventPeriod m_period;

        private bool m_bIsGroup = false;

        public TimedEvent()
        {
        }

        public TimedEvent(int p_Id, int p_Type, string p_SubjectType, int p_SubjectId, string p_Period, string p_Comment)
        {
            this.id = p_Id;
            this.subjectId = p_SubjectId;
            this.subjectType = p_SubjectType;
            this.period = p_Period;
        }

        public bool setData(TimedEvent dataEvent)
        {
            this.id = dataEvent.id;
            this.subjectId = dataEvent.subjectId;
            this.action = dataEvent.action;
            this.period = dataEvent.period;

            return true;
        }

        public int id
        {
            get { return m_iId; }
            set { m_iId = value; }
        }

        public string action
        {
            get { return m_sAction; }
            set { m_sAction = value; }
        }

        public string subjectType
        {
            get { return m_sSubjectType; }
            set { m_sSubjectType = value; }
        }

        public int subjectId
        {
            get { return m_iSubjectId; }
            set { m_iSubjectId = value; }
        }

        public string period
        {
            get { return m_period.value; }
            set { m_period = new TimedEventPeriod(value); }
        }

        //public System.DateTime timeStart
        //{
        //    get { return m_dTimeStart; }
        //    set { m_dTimeStart = value; }
        //}

        //public string timeStartStr
        //{
        //    get { return m_dTimeStart.ToString(Globals.DATETIME_FORMAT); }
        //    set { m_dTimeStart = System.DateTime.ParseExact(value, Globals.DATETIME_FORMAT, null); }
        //}

        //public System.DateTime timeEnd
        //{
        //    get { return m_dTimeEnd; }
        //    set { m_dTimeEnd = value; }
        //}

        //public string timeEndStr
        //{
        //    get { return m_dTimeEnd.ToString(Globals.DATETIME_FORMAT); }
        //    set { m_dTimeEnd = System.DateTime.ParseExact(value, Globals.DATETIME_FORMAT, null); }
        //}

        public bool isGroup
        {
            get { return m_bIsGroup; }
            set { m_bIsGroup = value; }
        }

        public string description
        {
            get
            {
                string sReturn = "";
                switch (this.action)
                {
                    case "target":
                        sReturn = "Target ";
                        break;
                    case "timed":
                        sReturn = "Timed ";
                        break;
                    case "off":
                        sReturn = "Off ";
                        break;
                }

                return sReturn + m_period.description;
            }
            set { ; }
        }

        public bool IsActive(DateTime p_Time, int startPeriod = 0)
        {
            return m_period.isActive(p_Time, startPeriod);
        }

        public bool IsPast(DateTime refTime)
        {
            return m_period.isPast(refTime);
        }

        public bool Save()
        {
            return true;
        }
    }

    public class TimedEventList : ObservableCollection<TimedEvent>
    {
    }
}

