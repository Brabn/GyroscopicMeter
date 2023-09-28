using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Threading;
namespace GyroControl
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            //linkLabel1.Links
        }

        private void btHelpFormClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:barabaniuk@gmail.com");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog saveLog = new SaveFileDialog();
            //saveLog.CreatePrompt = true;
            saveLog.OverwritePrompt = true;
            saveLog.FileName = "Gyro_control_1_3";
            saveLog.DefaultExt = "ino";
            saveLog.Filter =
                "Arduino files (*.ino)|*.ino|All files (*.*)|*.*";

            if (saveLog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveLog.FileName.Length > 0)
                using (StreamWriter sw = new StreamWriter(saveLog.FileName, true))
                {

                    sw.WriteLine(ArduinoText.Text);
                    sw.Close();
                }
        }
       
    }
}
