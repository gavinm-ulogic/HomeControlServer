using System;
using System.Threading;
using System.Collections.Generic;
using HomeControlServer.Models;


namespace HomeControlServer.Providers
{
    public class ProcessedRelay
    {
        public string relayAddress;
        public int eventId;

        public ProcessedRelay(string relayAddress, int eventId)
        {
            this.relayAddress = relayAddress;
            this.eventId = eventId;
        }
    }

    public static class TimerControl
    {
        private const int LOOP_DELAY = 40000;
        private static List<ProcessedRelay> m_oProcessedRelays = new List<ProcessedRelay>();

        private static bool m_bRun;
        private static int m_iTempDelta = 0;

        public static void Run()
        {
            Thread oWorker = null;
            oWorker = new Thread(WorkerThread);
            m_bRun = true;
            oWorker.Start();
        }

        public static void Kill()
        {
            m_bRun = false;
        }

        private static void WorkerThread()
        {
            Thread.Sleep(5000);
            // Let the program initialise before starting worker thread ops

            while (m_bRun)
            {
                Logger.Log(Logger.LOGLEVEL_INFO, "Before DoEvents");
                DoEvents();
                Logger.Log(Logger.LOGLEVEL_INFO, "After DoEvents");
                HeatingControl.Save();
                Thread.Sleep(LOOP_DELAY);
            }
        }

        public static void DoEvents()
        {
            try
            {
                RelayControl.ResetAllSetupRelays();
                foreach (TimedEvent timedEvent in HeatingControl.events)
                {
                    if (timedEvent.IsActive(DateTime.MinValue)) {
                        switch (timedEvent.subjectType)
                        {
                            case "heater":
                                Heater heater = HeatingControl.GetHeaterById(timedEvent.subjectId);
                                if (heater == null) { break; }
                                Logger.Log(Logger.LOGLEVEL_INFO, "Active timed event: heater: " + heater.name + ", relay: " + heater.relayAddress + 
                                    ", from " + timedEvent.timeStart.ToString("H:mm") + " to " + timedEvent.timeEnd.ToString("H:mm"));

                                ProcessHeaterEvent(heater, timedEvent);
                                break;
                        }
                    }
                }
                RelayControl.UpdateRelays();
            }
            catch (Exception ex)
            {
                var testVal = ex;
            }
        }

        //public static void DoHeating()
        //{
        //    try
        //    {
        //        List<TimedEvent> eventList;

        //        foreach (Heater heater in HeatingControl.heaters)
        //        {
        //            eventList = HeatingControl.getHeaterEvents(heater.id);
        //            foreach (TimedEvent timedEvent in eventList)
        //            {
        //                if (timedEvent.IsActive(DateTime.MinValue) && !string.IsNullOrEmpty(heater.relayAddress))
        //                {
        //                    ProcessHeaterEvent(heater, timedEvent);
        //                }

        //            }
        //        }

        //        RelayControl.UpdateRelays();
        //    }
        //    catch (Exception ex)
        //    {
        //        var testVal = ex;
        //    }
        //}

        private static void ProcessHeaterEvent(Heater heater, TimedEvent timedEvent)
        {
            bool newState = false;
            bool foundProcessed = false;

            int sensorTotal = 0;
            int sensorAverage = 0;
            foreach (Sensor sensor in heater.sensors)
            {
                sensorTotal += sensor.reading;
            }

            sensorAverage = sensorTotal / heater.sensors.Count;
            if (sensorAverage >= heater.tempMax) { return; }
            if (timedEvent.action == "timed")
            {
                newState = true;
            }
            else if (timedEvent.action == "target")
            {
                if (sensorAverage < heater.tempTarget + m_iTempDelta)
                {
                    newState = true;
                }
                else if (sensorAverage == heater.tempTarget + m_iTempDelta)
                { // Keep relay in same state to avoid flip-flopping
                    newState = RelayControl.GetRelayState(heater.relayAddress);
                }
            }

            foundProcessed = false;
            foreach (ProcessedRelay processedRelay in m_oProcessedRelays)
            {
                if (heater.relayAddress == processedRelay.relayAddress)
                {
                    if (timedEvent.id >= processedRelay.eventId)
                    {// highest event id has priority
                        processedRelay.eventId = timedEvent.id;
                    }
                    foundProcessed = true;
                    break;
                }
            }

            if (!foundProcessed)
            {
                m_oProcessedRelays.Add(new ProcessedRelay(heater.relayAddress, timedEvent.id));
            }

            RelayControl.SetupRelay(heater.relayAddress, newState);
        }
    }
}

