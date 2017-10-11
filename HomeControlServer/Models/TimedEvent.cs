using System;
using System.Collections.ObjectModel;
using HomeControlServer.Providers;

namespace HomeControlServer.Models
{
    public class TimedEvent
    {
        private int m_iId = 0;
        private string m_sSubjectType = "";
        private int m_iSubjectId = 0;
        private System.DateTime m_dTimeStart;
        private System.DateTime m_dTimeEnd;
        private string m_sAction = "off";       // off, timer (ignores sensors), target (uses sensors)

        private bool m_bIsGroup = false;

        public TimedEvent()
        {
        }

        public TimedEvent(int p_Id, int p_Type, string p_SubjectType, int p_SubjectId, string p_TimeStart, string p_TimeEnd, string p_Comment)
        {
            this.id = p_Id;
            this.subjectId = p_SubjectId;
            this.subjectType = p_SubjectType;
            try
            {
                timeStart = System.DateTime.ParseExact(p_TimeStart, Globals.DATETIME_FORMAT, null);
            }
            catch (Exception ex)
            {
                timeStart = System.DateTime.MinValue;
            }
            try
            {
                timeEnd = System.DateTime.ParseExact(p_TimeEnd, Globals.DATETIME_FORMAT, null);
            }
            catch (Exception ex)
            {
                timeEnd = System.DateTime.MinValue;
            }
        }

        public bool setData(TimedEvent dataEvent)
        {
            this.id = dataEvent.id;
            this.subjectId = dataEvent.subjectId;
            this.timeStart = dataEvent.timeStart;
            this.timeEnd = dataEvent.timeEnd;
            this.timeEnd = dataEvent.timeEnd;

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

        public System.DateTime timeStart
        {
            get { return m_dTimeStart; }
            set { m_dTimeStart = value; }
        }

        public string timeStartStr
        {
            get { return m_dTimeStart.ToString(Globals.DATETIME_FORMAT); }
            set { m_dTimeStart = System.DateTime.ParseExact(value, Globals.DATETIME_FORMAT, null); }
        }

        public System.DateTime timeEnd
        {
            get { return m_dTimeEnd; }
            set { m_dTimeEnd = value; }
        }

        public string timeEndStr
        {
            get { return m_dTimeEnd.ToString(Globals.DATETIME_FORMAT); }
            set { m_dTimeEnd = System.DateTime.ParseExact(value, Globals.DATETIME_FORMAT, null); }
        }

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

                string sDays = "";
                if (timeStart.Year < 1000)
                {// recuring event
                    if (timeStart.Year == 31)
                    {
                        sDays = "Week days";
                    }
                    else if (timeStart.Year == 96)
                    {
                        sDays = "Weekend";
                    }
                    else if (timeStart.Year == 96)
                    {
                        sDays = "All days";
                    }
                    else
                    {
                        if ((timeStart.Year & 1) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Mon";
                        }
                        if ((timeStart.Year & 2) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Tue";
                        }
                        if ((timeStart.Year & 4) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Wed";
                        }
                        if ((timeStart.Year & 8) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Thu";
                        }
                        if ((timeStart.Year & 16) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Fri";
                        }
                        if ((timeStart.Year & 32) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Sat";
                        }
                        if ((timeStart.Year & 64) > 0)
                        {
                            if (sDays != "") sDays += ", ";
                            sDays += "Sun";
                        }
                    }
                    if (sDays == "") sDays = "No days";
                    sReturn += sDays + " ";
                }
                sReturn += timeStart.ToString("t") + " to " + timeEnd.ToString("t");

                return sReturn;
            }
            set { ; }
        }

        public bool IsActive(DateTime p_Time)
        {
            int iEventDays = 0;
            int iNowDay = 0;

            if (p_Time == System.DateTime.MinValue)
            {
                p_Time = System.DateTime.Now;
            }
            iNowDay = (int)p_Time.DayOfWeek;
            if (iNowDay == 0) iNowDay = 7; // move sunday to after saturday
            iNowDay--; // mon = 0;
            int iNowDayBin = (int)Math.Pow(2, iNowDay);

            if (timeStart.Year < 1000)
            {
                // Not a fully specified range - i.e. repeating event
                iEventDays = timeStart.Year;

                if ((iNowDayBin & iEventDays) == 0)
                {
                    return false;
                }
                // got this far so it's the right day
                if (p_Time.TimeOfDay < timeStart.TimeOfDay | p_Time.TimeOfDay > timeEnd.TimeOfDay)
                {
                    return false;
                }
            }
            else
            {
                // Fully specified datetime range
                if (p_Time < timeStart)
                {
                    return false;
                } else if (p_Time > timeEnd)
                { // remove old event from list
                    HeatingControl.events.Remove(this);
                }
            }
            return true;
        }

        public bool IsPast(DateTime p_Time)
        {
            if (p_Time == System.DateTime.MinValue)
            {
                p_Time = System.DateTime.Now;
            }

            if (timeStart.Year < 1000)
            {
                // Repeating event 
                return false;
            }
            else
            {
                // Fully specified datetime range
                if (p_Time < timeEnd)
                {
                    return false;
                }
            }
            return true;
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

