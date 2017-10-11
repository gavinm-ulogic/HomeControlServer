using System;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using HomeControlServer.Models;

namespace HomeControlServer.Providers
{
    public static class SensorControl
    {
        private const int LOOP_DELAY = 120000;

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static bool m_bRun;
        public static void Run()
        {
            if (Environment.Version.Major >= 4)
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"..\Microsoft.NET\Framework\v2.0.50727");
                folder = Path.GetFullPath(folder);
                var dllFile = Path.Combine(folder, "vjsnativ.dll");
                var loadResult = LoadLibrary(dllFile);
                if (loadResult == IntPtr.Zero)
                {
                    Logger.Log(Logger.LOGLEVEL_ERROR, "###############################################################################################");
                    Logger.Log(Logger.LOGLEVEL_ERROR, "Failed to load DLL: " + dllFile);
                    Logger.Log(Logger.LOGLEVEL_ERROR, "1 wire sensors will not work!");
                    Logger.Log(Logger.LOGLEVEL_ERROR, "###############################################################################################");
                }
            }

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
            while (m_bRun)
            {

                try
                {
                    Logger.Log(Logger.LOGLEVEL_INFO, "Before sensors read");
                    PollSensors();
                    Logger.Log(Logger.LOGLEVEL_INFO, "After sensors read");
                    Thread.Sleep(LOOP_DELAY);
                }
                catch (Exception ex)
                {
                }
            }
        }
        
        private static dynamic getEnumerator(dynamic d)
        { // Remove J# dependency for getting the enumerator
            return d.getAllDeviceContainers();
        }

        private static void PollSensors()
        {
            com.dalsemi.onewire.container.OneWireContainer owd = default(com.dalsemi.onewire.container.OneWireContainer);
            object state = null;
            com.dalsemi.onewire.container.TemperatureContainer tc = default(com.dalsemi.onewire.container.TemperatureContainer);
            com.dalsemi.onewire.adapter.DSPortAdapter oAdapter = null;
            bool bFound1w = false;

            for (int i = 0; i < 16; i++)
            {
                try
                {
                    //Logger.Log(Logger.LOGLEVEL_INFO, "Looking for 1 wire on USB" + i.ToString());

                    oAdapter = null;
                    try { oAdapter = com.dalsemi.onewire.OneWireAccessProvider.getAdapter("{DS9490}", "USB" + i.ToString()); }
                    catch (Exception ex) 
                    {
                        string msg = ex.Message;
                        //Logger.Log(Logger.LOGLEVEL_INFO, "USB" + i.ToString() + " " + msg);
                    }

                    if (oAdapter != null)
                    {
                        bFound1w = true;
                        Logger.Log(Logger.LOGLEVEL_INFO, "Found 1 wire on USB" + i.ToString());
                        //java.util.Enumeration owd_enum = default(java.util.Enumeration);

                        // get exclusive use of 1-Wire network
                        oAdapter.beginExclusive(true);

                        // clear any previous search restrictions
                        oAdapter.setSearchAllDevices();
                        oAdapter.targetAllFamilies();
                        oAdapter.setSpeed(com.dalsemi.onewire.adapter.DSPortAdapter.SPEED_REGULAR);
                        //                        oAdapter.setSpeed(com.dalsemi.onewire.adapter.DSPortAdapter.SPEED_HYPERDRIVE);

                        // Get all device containers
                        //oAdapter.getAllDeviceContainers();

                        var owd_enum = getEnumerator(oAdapter);
                        //Logger.Log(Logger.LOGLEVEL_INFO, "Enumerating devices connected to adapter " + oAdapter.getAdapterName());

                        // enumerate through all the 1-Wire devices found (with Java-style enumeration)
                        while (owd_enum.hasMoreElements())
                        {
                            try
                            {
                                // retrieve OneWireContainer
                                owd = (com.dalsemi.onewire.container.OneWireContainer)owd_enum.nextElement();
                                // check to see if 1-Wire device supports temperatures, if so get address and temp.
                                if (owd is com.dalsemi.onewire.container.TemperatureContainer)
                                {
                                    // cast the OneWireContainer to TemperatureContainer
                                    tc = (com.dalsemi.onewire.container.TemperatureContainer)owd;
                                    // read the device
                                    state = tc.readDevice();
                                    // extract the temperature from previous read
                                    tc.doTemperatureConvert((sbyte[])state);
                                    SetValue(owd.getAddressAsString(), (int)tc.getTemperature((sbyte[])state));
                                }
                                else
                                {
                                    Logger.Log(Logger.LOGLEVEL_WARNING, "Non-thermometer device found");
                                }
                            }
                            catch (Exception ex)
                            {
                                string sError = "USB" + i.ToString() + " ";
                                try
                                {
                                    sError += owd.getAddressAsString() + " ";
                                }
                                catch (Exception ex2) 
                                {
                                    sError += "NO ID: " + ex2.Message + " ";
                                }
                                sError += ex.ToString();
                                    
                                Logger.Log(Logger.LOGLEVEL_ERROR, sError);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(Logger.LOGLEVEL_ERROR, "USB" + i.ToString() + " " + ex.ToString());
                }
                finally
                {
                    try
                    {
                        if (oAdapter != null)
                        {
                            // end exclusive use of 1-Wire net adapter
                            oAdapter.freePort();
                            oAdapter.endExclusive();
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            if (!bFound1w)
            {
                Logger.Log(Logger.LOGLEVEL_INFO, "No 1 wire USB found");
            }
        }

        private static void SetValue(String p_SensorId, int p_Value)
        {
            bool bFound = false;
            for (int i = 0; i < HeatingControl.sensors.Count; i++)
            {
                if (HeatingControl.sensors[i].sensorId == p_SensorId)
                {
                    HeatingControl.sensors[i].reading = p_Value;
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {// really shouldn't happen !!!!!!
                Logger.Log(Logger.LOGLEVEL_ERROR, "Missing sensor: " + p_SensorId);
            }
        }

        public static int GetValue(string p_SensorId)
        {
            for (int i = 0; i < HeatingControl.sensors.Count; i++)
            {
                if (HeatingControl.sensors[i].sensorId == p_SensorId) { return HeatingControl.sensors[i].reading; }
            }
            return Sensor.NO_READING;
        }
    }
}


