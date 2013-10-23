using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BlueRelayControllerDLL;

namespace BlueRelayController
{
    public partial class Form1 : Form
    {
        RelayCommands relay = new RelayCommands();

        public Form1()
        {
            InitializeComponent();
        }

        private void OpenDevice_Click(object sender, EventArgs e)
        {
            relay.OpenRelay();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                relay.RelayOn(1);
            else
                relay.RelayOff(1);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {                    
            if (checkBox2.Checked == true)
                relay.RelayOn(8);
            else
                relay.RelayOff(8);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
                relay.RelayOn(3);
            else
                relay.RelayOff(3);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
                relay.RelayOn(2);
            else
                relay.RelayOff(2);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
                relay.RelayOn(4);
            else
                relay.RelayOff(4);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
                relay.RelayOn(5);
            else
                relay.RelayOff(5);
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
                relay.RelayOn(6);
            else
                relay.RelayOff(6);
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
                relay.RelayOn(7);
            else
                relay.RelayOff(7);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            relay.AllRelaysOff();
        }

    }
}
