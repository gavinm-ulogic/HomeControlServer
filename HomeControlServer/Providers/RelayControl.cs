using System;
using System.Timers;

namespace HomeControlServer.Providers
{
    public static class RelayControl
    {

        private const int BOARDCOUNT = 3;

        private const int RELAYCOUNT = 8;
        //byte Data_value;
        //byte Address;
        private static byte Card_count;
        private static byte Address_Byte;
        private static byte Command_Byte;
        private static byte Data_byte;
        //byte CRC_byte;
        private static bool Delay_end;

        private static byte[] m_oLiveRelays = new byte[256];
        private static System.IO.Ports.SerialPort m_oSerialPort = new System.IO.Ports.SerialPort();
        //private static System.IO.Ports.SerialPort m_oSerialPort;

        private static System.Timers.Timer m_oTimer = new System.Timers.Timer();

        private static byte[] m_oSetupRelays = new byte[3];
        private static bool m_bDoInitialise = false;

        public static bool Initialise()
        {
            m_bDoInitialise = false;
            m_oSerialPort.Close();
            //m_oSerialPort = new System.IO.Ports.SerialPort();

            byte[] In_buffer = new byte[4];
            byte[] Send_buffer = new byte[1];
            Int16 i = default(Int16);
            Int16 x = default(Int16);

            string[] ports = null;
            string port = "";

            for (int iBoard = 0; iBoard <= BOARDCOUNT - 1; iBoard++)
            {
                m_oSetupRelays[iBoard] = 0;
            }

            m_oTimer.Elapsed += OnTimedEvent;


            try
            {
                ports = System.IO.Ports.SerialPort.GetPortNames();
                //Search available COM-Ports
                //If no COM-Port available
                if (ports.Length == 0)
                {
                    //					Interaction.MsgBox("No COM-PORT found", MsgBoxStyle.Critical);
                    System.Environment.Exit(0);
                }

                foreach (string port_loopVariable in ports)
                {
                    port = port_loopVariable;
                    try
                    {
                        var _with1 = m_oSerialPort;
                        _with1.PortName = port;
                        _with1.ReadTimeout = 500;
                        //Int32.Parse(500)
                        _with1.Open();


                    }
                    catch (Exception es)
                    {
                    }
                    finally
                    {
                        if (m_oSerialPort.IsOpen == true)
                        {
                            // found available port
                        }
                    }
                }

                if (m_oSerialPort.IsOpen == true)
                {
                    m_oSerialPort.Close();
                    // close com-port
                }

                m_oSerialPort.BaudRate = 19200;
                m_oSerialPort.Parity = System.IO.Ports.Parity.None;
                m_oSerialPort.DataBits = 8;
                m_oSerialPort.StopBits = System.IO.Ports.StopBits.One;
                //1 
                m_oSerialPort.Handshake = System.IO.Ports.Handshake.None;

                m_oSerialPort.PortName = port;

                m_oSerialPort.Open();
                // open Com-Port

                if (m_oSerialPort.IsOpen == false)
                {
                    m_bDoInitialise = true;
                    return false;
                }

                m_oSerialPort.DiscardInBuffer();

                // Initialize & synchronize cards if needed
                Send_buffer[0] = 1;

                for (x = 0; x <= 3; x++)
                {
                    m_oSerialPort.Write(Send_buffer, 0, 1);
                    sleep(4);
                    if (m_oSerialPort.BytesToRead > 3)
                    {
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }

                sleep(1023);
                // Wait for feedback

                Send_buffer[0] = 1;

                for (x = 0; x <= 3; x++)
                {
                    m_oSerialPort.Write(Send_buffer, 0, 1);
                }

                sleep(1023);
                // Wait for feedback

                // Get number of relay cards
                for (i = 0; i <= 255; i++)
                {
                    m_oSerialPort.Read(In_buffer, 0, 4);


                    if (In_buffer[0] == 1)
                    {
                        // if 255 cards connected  (answer of card no. 255 is zero)
                        if (In_buffer[1] == 0)
                        {
                            Card_count = 255;

                        }
                        else if (In_buffer[1] > 0)
                        {
                            Card_count = (byte)(In_buffer[(byte)1] - (byte)1);
                            // no. of relay cards: ddressbyte of last feedback-frame  - 1 
                        }

                        break; // TODO: might not be correct. Was : Exit For
                    }
                }

                // Test
                SetAllOff();
                SetOn(1, 7);
                sleep(1000);
                SetOff(1, 7);

                //for (i = 1; i <= 3; i++)
                //{
                //    for (int j = 0; j <= 7; j++)
                //    {
                //        SetOn(i, j);
                //        sleep(100);
                //        SetOff(i, j);
                //        sleep(100);
                //    }
                //}

            }
            catch (Exception ex)
            {// Error initialising card(s)
                m_bDoInitialise = true;
                return false;
            }
            return true;
        }


        public static void sleep(int Time)
        {
            m_oTimer.Interval = Time;
            m_oTimer.Start();

            Delay_end = false;

            while (!(Delay_end == true))
            {
                System.Threading.Thread.Sleep(1);
            }

            m_oTimer.Stop();
            Delay_end = false;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Delay_end = true;
        }

        public static byte Send_cmd(byte Address_Byte, byte Command_Byte, byte Data_byte)
        {
            if (m_bDoInitialise) { m_oSerialPort.Close(); Initialise(); }

            byte[] Send_buffer = new byte[4];
            byte[] In_buffer = new byte[1029];

            try
            {
                if (m_oSerialPort.IsOpen == false)
                {
                    //					Interaction.MsgBox("No Com-Port selcted", MsgBoxStyle.Exclamation);
                    return 0;
                }

                Send_buffer[0] = Command_Byte;
                //Create command-frame 
                Send_buffer[1] = Address_Byte;
                Send_buffer[2] = Data_byte;
                Send_buffer[3] = (byte)(Command_Byte ^ Address_Byte ^ Data_byte);
                //Calculate "checksum"

                m_oSerialPort.Write(Send_buffer, 0, 4);

                sleep(Card_count * 8 + 10);
                // delay for executing command & feedback (according to number of connected cards)
                m_oSerialPort.DiscardOutBuffer();
                m_oSerialPort.DiscardInBuffer();

                //GetSchaltzustände:
                //        Send_buffer(0) = 2                               ' Get Ports
                //        Send_buffer(1) = 0                               ' Broadcast
                //        Send_buffer(2) = 0
                //        Send_buffer(3) = Send_buffer(0) Xor Send_buffer(1) Xor Send_buffer(2)

                //        m_oSerialPort.Write(Send_buffer, 0, 4)

                //        sleep(Card_count * 8 + 10)                      ' delay for executing command & feedback (according to number of connected cards)
                //        m_oSerialPort.DiscardOutBuffer()

                //        For x As Byte = 0 To CByte(Card_count - 1)
                //            m_oSerialPort.Read(In_buffer, 0, 4)
                //            Relay_state_backup(x) = In_buffer(2)
                //            BINARYSTRING = Convert.ToString(In_buffer(2), 2)
                //            BINARYSTRING = BINARYSTRING.PadLeft(8, "0"c) ' convert to binary
                //        Next


            }
            catch (Exception ex)
            {
                //Interaction.MsgBox("Error executing command!", MsgBoxStyle.Exclamation);
                return 0;
            }
            return 1;
        }


        public static byte GetRelayState()
        {
            byte[] Send_buffer = new byte[4];
            byte[] In_buffer = new byte[1029];
            string BINARYSTRING = null;


            try
            {
                if (m_oSerialPort.IsOpen == false)
                {
                    //Interaction.MsgBox("No Com-Port selcted", MsgBoxStyle.Exclamation);
                    return 0;
                }

                sleep(Card_count * 8 + 10);
                // delay for executing command & feedback (according to number of connected cards)
                m_oSerialPort.DiscardOutBuffer();
                m_oSerialPort.DiscardInBuffer();

                Send_buffer[0] = 2;
                // Get Ports
                Send_buffer[1] = 0;
                // Broadcast
                Send_buffer[2] = 0;
                Send_buffer[3] = (byte)(Send_buffer[0] ^ Send_buffer[1] ^ Send_buffer[2]);

                m_oSerialPort.Write(Send_buffer, 0, 4);

                sleep(Card_count * 8 + 10);
                // delay for executing command & feedback (according to number of connected cards)
                m_oSerialPort.DiscardOutBuffer();

                for (byte x = 0; x <= Convert.ToByte(Card_count - 1); x++)
                {
                    m_oSerialPort.Read(In_buffer, 0, 4);
                    m_oLiveRelays[x] = In_buffer[2];
                    BINARYSTRING = Convert.ToString(In_buffer[2], 2);
                    BINARYSTRING = BINARYSTRING.PadLeft(8, '0');
                    // convert to binary
                }

            }
            catch (Exception ex)
            {
                //Interaction.MsgBox("Error executing command!", MsgBoxStyle.Exclamation);
                //m_oSerialPort.Close();
                //Initialise();
                m_bDoInitialise = true;
                return 0;
            }
            return 1;
        }



        private static void Shutdown()
        {
            // ERROR: Not supported in C#: OnErrorStatement

            //If Exit_All_Off.Checked And Exit_All_Off.Enabled = True Then   'All relays via Broadcast off on exit
            //    Address_Byte = 0
            //    Command_Byte = 3
            //    Data_byte = 0
            //    Send_cmd(Address_Byte, Command_Byte, Data_byte)
            //End If
            m_oSerialPort.Close();
            System.Environment.Exit(0);
        }

        public static void SetOn(string p_Relay)
        {
            string[] sAddress = p_Relay.Split('-'); // Strings.Split(p_Relay, "-");
            SetOn(Convert.ToInt32((sAddress[0])), Convert.ToInt32(sAddress[1]));
        }

        public static void SetOff(string p_Relay)
        {
            string[] sAddress = p_Relay.Split('-'); // Strings.Split(p_Relay, "-");
            SetOff(Convert.ToInt32((sAddress[0])), Convert.ToInt32(sAddress[1]));
        }

        private static void SetOn(int p_Card, int p_Relay)
        {
            Address_Byte = Convert.ToByte(p_Card);
            Command_Byte = 6;
            Data_byte = Convert.ToByte(Math.Pow(2, p_Relay));
            Send_cmd(Address_Byte, Command_Byte, Data_byte);
        }

        private static void SetOff(int p_Card, int p_Relay)
        {
            Address_Byte = Convert.ToByte(p_Card);
            Command_Byte = 7;
            Data_byte = Convert.ToByte(Math.Pow(2, p_Relay));
            Send_cmd(Address_Byte, Command_Byte, Data_byte);
        }

        private static void SetRelays(byte p_Card, byte p_State)
        {
            string sState = Convert.ToString(p_State, 2);
            sState = sState.PadLeft(8, '0');
            string sStateRev = Logger.Reverse(sState);
            Logger.Log(Logger.LOGLEVEL_INFO, "SetRelays, card: " + p_Card.ToString() + " - " + sStateRev);
            Address_Byte = Convert.ToByte(p_Card);
            Command_Byte = 3;
            Data_byte = p_State;
            Send_cmd(Address_Byte, Command_Byte, Data_byte);
        }

        public static void SetAllOff()
        {
            Address_Byte = 0; // Address;
            // selected card / Broadcast
            Command_Byte = 3;
            // Set-Ports
            Data_byte = 0;
            // Rel. 1-8 off
            Send_cmd(Address_Byte, Command_Byte, Data_byte);
        }

        public static void SetupRelay(string p_Address, bool p_State)
        {
            string[] saAddress = p_Address.Split('-');
            byte byteBoard = byte.Parse(saAddress[0]);
            byte byteRelay = byte.Parse(saAddress[1]);
            byte byteMask = (byte)(1 << byteRelay);

            if (p_State)
            {
                m_oSetupRelays[byteBoard - 1] = (byte)(m_oSetupRelays[byteBoard - 1] | byteMask);
            }
            else
            {
                m_oSetupRelays[byteBoard - 1] = (byte)(m_oSetupRelays[byteBoard - 1] & (~byteMask));
            }
        }

        public static byte GetBoardState(byte p_Board)
        {
            return m_oLiveRelays[p_Board - 1];
        }

        public static bool GetRelayState(string p_Address)
        {
            string[] saAddress = p_Address.Split('-');
            byte byteBoard = byte.Parse(saAddress[0]);
            byte byteRelay = byte.Parse(saAddress[1]);

            return GetRelayState(byteBoard, byteRelay);
        }

        public static bool GetRelayState(byte p_Board, byte p_Relay)
        {
            byte byteMask = (byte)(1 << p_Relay);

            return ((m_oLiveRelays[p_Board - 1] & byteMask) > 0);
        }

        public static void UpdateRelays()
        {
            for (int iBoard = 0; iBoard <= BOARDCOUNT - 1; iBoard++)
            {
                if (m_oSetupRelays[iBoard] != m_oLiveRelays[iBoard])
                {
                    SetRelays((byte)(iBoard + 1), m_oSetupRelays[iBoard]);
                }
            }
            GetRelayState();
        }

        public static void CopyRelaysToSetup()
        {
            for (int iBoard = 0; iBoard <= BOARDCOUNT - 1; iBoard++)
            {
                m_oSetupRelays[iBoard] = m_oLiveRelays[iBoard];
            }
        }

        public static void ResetAllSetupRelays()
        {
            for (int iBoard = 0; iBoard <= BOARDCOUNT - 1; iBoard++)
            {
                m_oSetupRelays[iBoard] = 0;
            }
        }

        public static bool IsLive()
        {
            //GetRelayState();
            //byte byTest = 0;

            //for (byte byBoard = 0; byBoard <= BOARDCOUNT - 1; byBoard++)
            //{
            //    byTest = GetBoardState((byte)(byBoard + 1));
            //    if (byTest != m_oSetupRelays[byBoard]) return false;
            //}
            return !m_bDoInitialise;
        }
    }
}
