using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BlueRelayControllerDLL;
using FTD2XX_NET;
using System.Collections;
using System.Threading;

enum CheckBoxes
{
    relay1, relay2, relay3, relay4, relay5, relay6, relay7, relay8
}

namespace BlueRelayController
{
    public partial class Form1 : Form
    {
        private const string BLUE_RELAY_MUTEX_NAME = "BlueRelayMutex";
        RelayCommands relay = new RelayCommands();
        private byte[] _sentBytes = new byte[2];
        private uint _receivedBytes;
        private FTDI.FT_STATUS _ftStatus;
        private RelayState m_RelayState;
        //FTDI newRelay = new FTDI();

        public Form1()
        {
            InitializeComponent();
        }

        private void OpenDevice_Click(object sender, EventArgs e)
        {
            relay.OpenRelay();
        }

        //private void checkBox1_CheckedChanged(object sender, EventArgs e)
        //{
            
        //    if (relay1.Checked)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 1, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 1, false);
        //}

        //private void checkBox2_CheckedChanged(object sender, EventArgs e)
        //{                    
        //    if (relay8.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 8, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 8, true);
        //}

        //private void checkBox3_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay3.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 3, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 3, false);
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //private void checkBox4_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay2.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 2, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 2, false);
        //}

        //private void checkBox5_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay4.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 4, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 4, false);
        //}

        //private void checkBox6_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay5.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 5, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 5, false);
        //}

        //private void checkBox7_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay6.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 6, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 6, false);
        //}

        //private void checkBox8_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (relay7.Checked == true)
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 7, true);
        //    else
        //        SetRelayPort(comboBox2.SelectedItem.ToString(), 7, false);
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateCheckboxes();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            relay.SendChar(textBox1.Text);
        }

        private void MultiRelayTest()
        {
            try
            {
                FTDI myFtdi_0 = new FTDI();
                FTDI myFtdi_1 = new FTDI();

                Console.WriteLine("Turning on FTDI 0...");
                OpenRelay(myFtdi_0, 0);

                Console.WriteLine("Turning on FTDI 1...");
                OpenRelay(myFtdi_1, 1);

                Console.WriteLine("Turning on [FTDI_0]:[Relay_1]");
                RelayOn(myFtdi_0, 1);

                Console.WriteLine("Turning on [FTDI_1]:[Relay_1]");
                RelayOn(myFtdi_1, 1);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception occured: " + e.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MultiRelayTest();
        }

        //Opens the com port to the relay
        public void OpenRelay(FTDI ftdi,uint deviceIndex)
        {
            _ftStatus = ftdi.OpenByIndex(deviceIndex);

            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new Exception("Could not open the device: " + _ftStatus);
            }

            _ftStatus = ftdi.SetBaudRate(921600);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                throw new Exception("Failed to set baudrate (error " + _ftStatus + ")");
            }

            _ftStatus = ftdi.SetBitMode(255, 4);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                throw new Exception("Failed to set bit mode (error " + _ftStatus + ")");
            }

            _sentBytes[0] = 0;

            // Get device info
            string serialNumber = "";
            ftdi.GetSerialNumber(out serialNumber);

            MessageBox.Show("Relay #" + deviceIndex + " Serial: " + serialNumber);
        }

        public void RelayOn(FTDI ftdi, int relayNum)
        {
            int pow = relayNum - 1;
            int command = (int)Math.Pow(2, pow);
            byte toSend = (byte)command;
            _sentBytes[0] = (byte)(_sentBytes[0] | toSend);

            if (ftdi.Write(_sentBytes, 1, ref _receivedBytes) != FTDI.FT_STATUS.FT_OK)
            {
                throw new Exception("Failed to turn on relay");
            }
        }

        // ------------------------------------------------------------------------------------------------------------------
        // --------------------------------------------New Code--------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------

        private const string BLUE_RELAY_MUTEX = "BlueRelayMutex";
        Mutex mutex = new Mutex();
        private static int MUTEX_TIMEOUT = 5000;

        private void Form1_Load(object sender, EventArgs e)
        {
            //relay.OpenRelay();
            for (int i = 0; i < 256; i++)
                comboBox1.Items.Add(i);
            UpdateConnectedRelays();

        }
        //private delegate void UpdateConnectedRelays();
        private void UpdateConnectedRelays()
        {
            comboBox2.Items.Clear();
            if (GetConnectedRelays().Length == 0)
            {
                comboBox2.Items.Add("No relay detected");
                foreach (Control c in tabPage1.Controls)
                {
                    if (c is CheckBox)
                        c.Enabled = false;
                }
            }
            else
            {
                foreach (string serial in GetConnectedRelays())
                {
                    comboBox2.Items.Add(serial);
                }
                foreach (Control c in tabPage1.Controls)
                {
                    if (c is CheckBox)
                        c.Enabled = true;
                }
            }

            comboBox2.SelectedIndex = 0;
        }

        //returns a string array of all the connected devices
        public string[] GetConnectedRelays()
        {
            ArrayList serials = new ArrayList();
            FTDI relay = GetRelayInstance();
            FTDI.FT_DEVICE_INFO_NODE[] list =  new FTDI.FT_DEVICE_INFO_NODE[100];
            relay.GetDeviceList(list);
            foreach (FTDI.FT_DEVICE_INFO_NODE tmp in list)
            {
                if (tmp != null)
                {
                    serials.Add(tmp.SerialNumber);
                }
            }
            string[] serialsArray = new string[serials.Count];
            int i = 0;
            foreach (string serial in serials)
            {
                serialsArray[i] = serial;
                i++;
            }
            return serialsArray;
        }

        //handles complete atomic setting of a relay
        public FTDI.FT_STATUS SetRelayPort(string relaySerial, int relayPort, bool status)
        {
            byte[] sentBytes = new byte[2];
            uint receivedBytes = 0;
            FTDI activeRelay = new FTDI();

            try
            {
                activeRelay = OpenRelay(relaySerial);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            }

                lock (activeRelay)
                {
                    try
                    {
                        //get current relay status
                        byte currentStatus = (byte)GetRelayStatus(activeRelay);
                        // calculate the exact commands to be sent
                        int pow = relayPort - 1;
                        //int command = Power(2, pow);
                        int command = (int)Math.Pow((double)2, (double)pow);
                        //XOR between current status and requested bit
                        sentBytes[0] = (byte)(command ^ currentStatus);

                        FTDI.FT_STATUS ftdiStatus = activeRelay.Write(sentBytes, 1, ref receivedBytes);

                        if (ftdiStatus != FTDI.FT_STATUS.FT_OK)
                        {
                            throw new Exception("Bad status: " + ftdiStatus);
                        }
                    }

                    catch (Exception RelayNotOpen)
                    {
                        Console.WriteLine(RelayNotOpen.Message);
                    }
                    finally
                    {
                        CloseRelay(activeRelay);
                    }
                }

            return FTDI.FT_STATUS.FT_OK;
        }

        //calculates a number to a power of another power
        private int Power(int baseNum, int exp)
        {
            //handle to the power of zero and 1;
            if (exp == 0)
                return 1;
            if (exp == 1)
                return baseNum;
            //higher numbers
            int ret = baseNum;
            for (int i = 1; i < exp; i++)
            {
                ret = ret * baseNum;
            }

            return ret;
        }

        //assumption - every relay action is atomic.
        //a each relay action consists of open, set and close operations.


        public FTD2XX_NET.FTDI OpenRelay(string relaySerial)
        {
            FTDI newRelay = GetRelayInstance();
            try
            {

                Console.WriteLine(" --> OpenRelay: " + relaySerial);

                if (newRelay.OpenBySerialNumber(relaySerial) != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("relay.OpenBySerialNumber() failed.");
                }


                _ftStatus = newRelay.SetBaudRate(921600);
                if (_ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    throw new Exception("Failed to set baudrate (error " + _ftStatus + ")");
                }

                _ftStatus = newRelay.SetBitMode(255, 4);
                if (_ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    throw new Exception("Failed to set bit mode (error " + _ftStatus + ")");
                }

            }
            catch (Exception FailedToOpenRelay)
            {
                Console.WriteLine(FailedToOpenRelay.Message);
                return null;
            }
            return newRelay;
        }

        public void CloseRelay(FTDI relay)
        {
            try
            {
                string deviceSerialNum = "";
                relay.GetSerialNumber(out deviceSerialNum);
                Console.WriteLine(" <-- CloseRelay: " + deviceSerialNum);
                if (relay.Close() != FTDI.FT_STATUS.FT_OK)
                {
                    MessageBox.Show("relay.Close() failed.");
                }

                //if (relay.IsOpen)
                //    MessageBox.Show("failed to close relay after writing");
            }
            catch (Exception RelayNotOpen)
            {
                Console.WriteLine(RelayNotOpen.Message);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public uint GetRelayStatus(FTDI relay)
        {
            byte relayStatus = 0;
            try
            {
                relay.GetPinStates(ref relayStatus);
            }
            catch (Exception RelayNotOpen)
            {
                Console.WriteLine(RelayNotOpen.Message);
                return 260;
            }
            return (uint)relayStatus;
        }
        
        private void button5_Click_1(object sender, EventArgs e)
        {
            UpdateConnectedRelays();
            UpdateCheckboxesWithoutEventInvoke(UpdateConnectedRelays);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            uint RecievedBytes = 0;
            byte[] toSend = new byte[2];
            toSend[0] = byte.Parse(comboBox1.SelectedItem.ToString());
            FTDI relay = GetRelayInstance();
            relay.OpenBySerialNumber(comboBox2.SelectedItem.ToString());
            relay.Write(toSend, 1, ref RecievedBytes);
            relay.Close();
            UpdateCheckboxes();

        }

        //used for updating the checkboxes without invoking an event
        private void UpdateCheckboxesWithoutEventInvoke(Action todo)
        {
            foreach (Control c in this.tabPage1.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox thisCheck = (CheckBox)c;
                    thisCheck.CheckedChanged -= relay_CheckedChanged;
                }
            }
            
            todo.Invoke();

            foreach (Control c in this.tabPage1.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox thisCheck = (CheckBox)c;
                    thisCheck.CheckedChanged -= relay_CheckedChanged;
                    thisCheck.CheckedChanged += relay_CheckedChanged;
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCheckboxesWithoutEventInvoke(UpdateCheckboxes);
        }

        private void UpdateCheckboxes()
        {
            //איכס איכס איכס
            
            FTDI relay = OpenRelay(comboBox2.SelectedItem.ToString());
            if (relay == null)
                return;

            _ftStatus = relay.SetBaudRate(921600);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                throw new Exception("Failed to set baudrate (error " + _ftStatus + ")");
            }

            _ftStatus = relay.SetBitMode(255, 4);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                throw new Exception("Failed to set bit mode (error " + _ftStatus + ")");
            }

            uint status = GetRelayStatus(relay);
            //MessageBox.Show("byte recieved: " + status.ToString());

            string s = Convert.ToString(status, 2); //Convert to binary in a string

            int[] bits = s.PadLeft(8, '0') // Add 0's from left
                         .Select(c => int.Parse(c.ToString())) // convert each char to int
                         .ToArray(); // Convert IEnumerable from select to Array
            Array.Reverse(bits);


            if (bits[0] == 1)
                relay1.Checked = true;
            else
                relay1.Checked = false;

            if (bits[1] == 1)
                relay2.Checked = true;
            else
                relay2.Checked = false;

            if (bits[2] == 1)
                relay3.Checked = true;
            else
                relay3.Checked = false;

            if (bits[3] == 1)
                relay4.Checked = true;
            else
                relay4.Checked = false;

            if (bits[4] == 1)
                relay5.Checked = true;
            else
                relay5.Checked = false;

            if (bits[5] == 1)
                relay6.Checked = true;
            else
                relay6.Checked = false;

            if (bits[6] == 1)
                relay7.Checked = true;
            else
                relay7.Checked = false;

            if (bits[7] == 1)
                relay8.Checked = true;
            else
                relay8.Checked = false;

            CloseRelay(relay);
        }

        //acquire Mutex for the dll and return an object
        public FTDI GetRelayInstance()
        {
            Console.WriteLine("attempting to acquire mutex");
            mutex = AcquireMutex(BLUE_RELAY_MUTEX_NAME);
            Console.WriteLine("mutex acquired");
            FTDI relay = new FTDI();
            return relay;
        }

        private void relay_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox cb = (CheckBox)sender;

                if (cb.Checked == true)
                    SetRelayPort(comboBox2.SelectedItem.ToString(), int.Parse(cb.Tag.ToString()), true);
                else
                    SetRelayPort(comboBox2.SelectedItem.ToString(), int.Parse(cb.Tag.ToString()), false);
            }
            catch (Exception noRelayAcquiredYet)
            {
                Console.WriteLine("no relay acquired before press");
            }
        }

        /// <summary>
        /// Acquire a system-wide mutex with the specified name.
        /// </summary>
        /// <param name="mutexName">The system-wide mutex name</param>
        /// <returns></returns>
        private Mutex AcquireMutex(string mutexName)
        {
            Console.WriteLine("[AcquireMutex] Trying to acquire mutex: {0}", mutexName);

            bool createdNew;

            Mutex mutex = new Mutex(true, mutexName, out createdNew);

            // (from MSDN)
            // This thread owns the mutex only if it both requested  
            // initial ownership and created the named mutex. Otherwise, 
            // it can request the named mutex by calling WaitOne. 
            if (!createdNew)
            {
                if (!mutex.WaitOne(MUTEX_TIMEOUT))
                    throw new Exception("Mutex not acquired.");
            }

            return mutex;
        }

    }
}
