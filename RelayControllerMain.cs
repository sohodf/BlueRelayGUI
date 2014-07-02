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
using BlueRelayController;
using System.Timers;


namespace BlueRelayController
{
    public partial class RelayControllerMain : Form
    {
        RelayActions RA = new RelayActions();
        private FTDI.FT_STATUS _ftStatus;

        public RelayControllerMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //refreshes checkboxes without invoking relay related action
        private void button2_Click(object sender, EventArgs e)
        {
            ExecuteActionWithoutCheckboxEventInvoke(UpdateCheckboxes);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateConnectedRelays();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Tick+= new EventHandler(timer1_Tick);
            timer.Start(); 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ExecuteActionWithoutCheckboxEventInvoke(UpdateCheckboxes);
            //Console.WriteLine("[tick] - checkboxes updated");
        }

        //updates connected relays in the combo box and enables checkboxes.
        private void UpdateConnectedRelays()
        {
            comboBox2.Items.Clear();
            if (RA.GetConnectedRelays().Length == 0)
            {
                comboBox2.Items.Add("No relay detected");
                foreach (Control c in groupBox1.Controls)
                {
                    if (c is CheckBox)
                        c.Enabled = false;
                }
            }
            else
            {
                foreach (string serial in RA.GetConnectedRelays())
                {
                    comboBox2.Items.Add(serial);
                }
                foreach (Control c in groupBox1.Controls)
                {
                    if (c is CheckBox)
                        c.Enabled = true;
                }
            }

            comboBox2.SelectedIndex = 0;
        }
        
        private void button5_Click_1(object sender, EventArgs e)
        {
            ExecuteActionWithoutCheckboxEventInvoke(UpdateConnectedRelays);
        }


        //used for updating the checkboxes without invoking an event
        private void ExecuteActionWithoutCheckboxEventInvoke(Action todo)
        {
            foreach (Control c in this.groupBox1.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox thisCheck = (CheckBox)c;
                    thisCheck.CheckedChanged -= relay_CheckedChanged;
                }
            }
            
            todo.Invoke();

            foreach (Control c in this.groupBox1.Controls)
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
            
        }


        private uint getRelayStatus()
        {
            FTDI relay = RA.OpenRelay(comboBox2.GetItemText(comboBox2.SelectedItem));
            uint status = RA.GetRelayStatus(relay);
            RA.CloseRelay(relay);
            return status;
        }

        private void UpdateCheckboxes()
        {
            uint status;
           
            status = RA.MutexWrapperWithRetval<uint>(() => getRelayStatus());

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

            
        }


        private void relay_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox cb = (CheckBox)sender;

                if (cb.Checked == true)
                    RA.SetRelayPort(comboBox2.SelectedItem.ToString(), int.Parse(cb.Tag.ToString()), true);
                else
                    RA.SetRelayPort(comboBox2.SelectedItem.ToString(), int.Parse(cb.Tag.ToString()), false);
            }
            catch (Exception noRelayAcquiredYet)
            {
                Console.WriteLine("no relay acquired before press");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            RA.WriteByteToRelay(comboBox2.SelectedItem.ToString(), 255);
            ExecuteActionWithoutCheckboxEventInvoke(UpdateCheckboxes);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RA.WriteByteToRelay(comboBox2.SelectedItem.ToString(), 0);
            ExecuteActionWithoutCheckboxEventInvoke(UpdateCheckboxes);
        }

        private void comboBox2_MouseClick(object sender, MouseEventArgs e)
        {
            ExecuteActionWithoutCheckboxEventInvoke(UpdateCheckboxes);
        }

    }
}
