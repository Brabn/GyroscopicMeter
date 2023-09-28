using System;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.ComponentModel;

using System.Windows;

using System.Threading;
using System.Windows.Forms;
//*****************************************************************************************
//                           LICENSE INFORMATION
//*****************************************************************************************
//   PCCom.SerialCommunication Version 1.0.0.0
//   Class file for managing serial port communication
//
//   Copyright (C) 2007  
//   Richard L. McCutchen 
//   Email: richard@psychocoder.net
//   Created: 20OCT07
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//*****************************************************************************************
namespace GyroControl
{
    class CommunicationManager
    {
        #region Manager Enums
        /// <summary>
        /// enumeration to hold our transmission types
        /// </summary>
        public enum TransmissionType { Text, Hex }

        /// <summary>
        /// enumeration to hold our message types
        /// </summary>
        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };
        #endregion

        #region Manager Variables
        //property variables
        private string _baudRate = string.Empty;
        private string _parity = string.Empty;
        private string _stopBits = string.Empty;
        private string _dataBits = string.Empty;
        private string _portName = string.Empty;
        private TransmissionType _transType;
        private RichTextBox _displayWindow;
        public RichTextBox SensorLog;
        public Label M1power;
        public GyroForm parentForm;
        public int Rows = 2;
        public string Sensors;
        public string[] p;
        public string p1;
        //global manager variables
        private Color[] MessageColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        public SerialPort comPort = new SerialPort();
        #endregion

        #region Manager Properties
        /// <summary>
        /// Property to hold the BaudRate
        /// of our manager class
        /// </summary>
        public string BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// property to hold the Parity
        /// of our manager class
        /// </summary>
        public string Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// property to hold the StopBits
        /// of our manager class
        /// </summary>
        public string StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        /// <summary>
        /// property to hold the DataBits
        /// of our manager class
        /// </summary>
        public string DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// property to hold the PortName
        /// of our manager class
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// property to hold our TransmissionType
        /// of our manager class
        /// </summary>
        public TransmissionType CurrentTransmissionType
        {
            get { return _transType; }
            set { _transType = value; }
        }

        /// <summary>
        /// property to hold our display window
        /// value
        /// </summary>
        public RichTextBox DisplayWindow
        {
            get { return _displayWindow; }
            set { _displayWindow = value; }
        }
        #endregion

        #region Manager Constructors
        /// <summary>
        /// Constructor to set the properties of our Manager Class
        /// </summary>
        /// <param name="baud">Desired BaudRate</param>
        /// <param name="par">Desired Parity</param>
        /// <param name="sBits">Desired StopBits</param>
        /// <param name="dBits">Desired DataBits</param>
        /// <param name="name">Desired PortName</param>
        public CommunicationManager(string baud, string par, string sBits, string dBits, string name, RichTextBox rtb)
        {
            _baudRate = baud;
            _parity = par;
            _stopBits = sBits;
            _dataBits = dBits;
            _portName = name;
            _displayWindow = rtb;
            //now add an event handler

            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
        }

        /// <summary>
        /// Comstructor to set the properties of our
        /// serial port communicator to nothing
        /// </summary>
        public CommunicationManager()
        {
            _baudRate = string.Empty;
            _parity = string.Empty;
            _stopBits = string.Empty;
            _dataBits = string.Empty;
            _portName = "COM1";
            _displayWindow = null;
            //add event handler
            //parentForm = parent;
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
        }
        #endregion

        #region WriteData
        public void WriteData(string msg)
        {
            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    //first make sure the port is open
                    //if its not open then open it
                    if (!(comPort.IsOpen == true)) comPort.Open();
                    //send the message to the port
                    comPort.Write(msg);
                    //display the message
                    DisplayData(MessageType.Outgoing, "" + msg + "\n");
                    break;
                case TransmissionType.Hex:
                    try
                    {
                        //convert the message to byte array
                        byte[] newMsg = HexToByte(msg);
                        //send the message to the port
                        comPort.Write(newMsg, 0, newMsg.Length);
                        //convert back to hex and display
                        //DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "\n");
                        DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "");
                    }
                    catch (FormatException ex)
                    {
                        //display error message
                        DisplayData(MessageType.Error, ex.Message);
                    }
                    finally
                    {
                        _displayWindow.SelectAll();
                    }
                    break;
                default:
                    //first make sure the port is open
                    //if its not open then open it
                    if (!(comPort.IsOpen == true)) comPort.Open();
                    //send the message to the port
                    comPort.Write(msg);
                    //display the message
                    DisplayData(MessageType.Outgoing, msg + "");
                    //DisplayData(MessageType.Outgoing, msg );

                    break;
            }
        }
        #endregion

        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        #endregion

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion

        #region DisplayData
        /// <summary>
        /// method to display the data to & from the port
        /// on the screen
        /// </summary>
        /// <param name="type">MessageType of the message</param>
        /// <param name="msg">Message to display</param>
        [STAThread]
        private void DisplayData(MessageType type, string msg)
        {
            bool sMsg = false;
            _displayWindow.Invoke(new EventHandler(delegate
            {
                _displayWindow.SelectedText = string.Empty;
                _displayWindow.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
                _displayWindow.SelectionColor = MessageColor[(int)type];
                _displayWindow.AppendText(msg);
                _displayWindow.ScrollToCaret();
            }));
            //string sensorMsg = msg.Replace(",", ", ");
            //if (sensorMsg.Substring(0, 2) == "t=")
            //    type = MessageType.Warning ;


        }
        #endregion

        #region OpenPort

        public bool OpenPort()
        {
            try
            {
                //first check if the port is already open
                //if its open then close it
                if (comPort.IsOpen == true) comPort.Close();

                //set the properties of our SerialPort Object
                comPort.BaudRate = int.Parse(_baudRate);    //BaudRate
                comPort.DataBits = int.Parse(_dataBits);    //DataBits
                comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBits);    //StopBits
                comPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);    //Parity
                /*comPort.BaudRate = 9600;    //BaudRate
                comPort.DataBits = 8;    //DataBits
                comPort.StopBits = 1;    //StopBits
                comPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None");*/
                comPort.PortName = _portName;   //PortName
                //now open the port
                comPort.Open();
                //display message
                DisplayData(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");
                //return true
                return true;
            }
            catch (Exception ex)
            {
                DisplayData(MessageType.Error, ex.Message);
                return false;
            }
        }
        #endregion
        #region ClosePort

        public bool ClosePort()
        {
            try
            {
                //first check if the port is already open
                //if its open then close it
                if (comPort.IsOpen == true) comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                DisplayData(MessageType.Error, ex.Message);
                return false;
            }
        }
        #endregion
        #region SetParityValues
        public void SetParityValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region SetStopBitValues
        public void SetStopBitValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(StopBits)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region SetPortNameValues
        public void SetPortNameValues(ref string[] portlist)
        {

            foreach (string str in SerialPort.GetPortNames())
            {
                //((ComboBox)obj).Items.Add(str);
                                Array.Resize(ref portlist, portlist.Length + 1);
                                portlist[portlist.Length - 1] = str;
                
            }
        }
        #endregion

        #region comPort_DataReceived
        /// <summary>
        /// method that will be called when theres data waiting in the buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetTextSafe(Label label, string newtext)
        {
            if (label.InvokeRequired)
                label.Invoke(new Action<string>((s) => label.Text = s), newtext);
            else label.Text = newtext;
        }
        delegate void SetTextCallback(string text);
        private void SetTextP1(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.M1power.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetTextP1);
                M1power.Invoke(d, new object[] { text });
            }
            else
            {
                this.M1power.Text = text;
            }
        }
        void UpdateLabels(string msg)
        {

            string[] stringSeparators = new string[] { "\r\n" };

            string[] sensorMsg = msg.Split(stringSeparators, StringSplitOptions.None);
            if (sensorMsg.Length > 2)
            {
                if (sensorMsg[sensorMsg.Length - 1] == "")
                    Sensors = sensorMsg[sensorMsg.Length - 2];
                else
                    Sensors = sensorMsg[sensorMsg.Length - 1];
                p = Sensors.Split(',');
                p1 = p[0].Split('=')[1];
                Rows = sensorMsg.Length;
                SensorLog.Invoke(new EventHandler(delegate
                {
                    SensorLog.SelectedText = string.Empty;
                    //SensorLog.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
                    // SensorLog.SelectionColor = MessageColor[(int)type];
                    SensorLog.AppendText(p[0]);
                    SensorLog.ScrollToCaret();

                }));
            }
            /*if (sensorMsg.Length > Rows)
            {
                Sensors = sensorMsg[sensorMsg.Length - 1];
                p = Sensors.Split(',');
                p1 = p[0];
                Rows = sensorMsg.Length;
               // SetTextSafe(M1power, p1);
                M1power.Invoke(new EventHandler(delegate
                {
                    // string[] p=Sensors.Split(',');
                    M1power.Text = p1;
                    //M1power.Text = "Cahanged";
                }));
            }*/

        }
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //determine the mode the user selected (binary/string)
            switch (CurrentTransmissionType)
            {
                //user chose string
                case TransmissionType.Text:
                    //read data waiting in the buffer
                    string msg = comPort.ReadExisting();
                    //display the data to the user
                    DisplayData(MessageType.Incoming, msg);
                    //UpdateLabels( msg);
                    //SensorLog.AppendText(msg);
                    // string[] stringSeparators = new string[] { "\r\n" };
                    //string[] sensorMsg = msg.Split(stringSeparators, StringSplitOptions.None);
                    /* if (sensorMsg.Length > Rows)
                     {

                         Rows = sensorMsg.Length;
                     }*/

                    // string currMsg = sensorMsg[sensorMsg.Length - 2];
                    try
                    {
                        parentForm.Invoke(new EventHandler(delegate
                        {

                            parentForm.InvokedMessage();
                            //parentForm.Update();
                        }));
                    }
                    catch
                    {
                    }
                    
                    //parentForm.EndInvoke();
                    /* parentForm.BeginInvoke(new MethodInvoker(delegate
                     {
                         parentForm.InvokedMessage(msg);
                     }));   */

                    /* parentForm.Invoke(new EventHandler(delegate
                     {

                         parentForm.InvokedMessage(msg);

                     }));*/
                    /*string[] sensorMsg = msg.Split(stringSeparators, StringSplitOptions.None);
                    if (sensorMsg.Length > 2)
                    {
                        if (sensorMsg[sensorMsg.Length - 1] == "")
                            Sensors = sensorMsg[sensorMsg.Length - 2];
                        else
                            Sensors = sensorMsg[sensorMsg.Length - 1];
                        p = Sensors.Split(',');
                        p1 = p[0].Split('=')[1];
                        Rows = sensorMsg.Length;
   
                    }*/
                    /*SensorLog.Invoke(new EventHandler(delegate
                       {
                           SensorLog.SelectedText = string.Empty;
                           //SensorLog.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
                           // SensorLog.SelectionColor = MessageColor[(int)type];
                           SensorLog.AppendText(msg);
                           SensorLog.ScrollToCaret();

                       }));*/
                    //if (sensorMsg.Substring(0, 2) == "t")
                    /* {
                         SensorLog.Invoke(new EventHandler(delegate
                         {
                             SensorLog.SelectedText = string.Empty;
                             //SensorLog.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
                             // SensorLog.SelectionColor = MessageColor[(int)type];
                             SensorLog.AppendText(msg);
                             SensorLog.ScrollToCaret();

                         }));
                     }*/

                    break;
                //user chose binary
                case TransmissionType.Hex:
                    //retrieve number of bytes in the buffer
                    int bytes = comPort.BytesToRead;
                    //create a byte array to hold the awaiting data
                    byte[] comBuffer = new byte[bytes];
                    //read the data and store it
                    comPort.Read(comBuffer, 0, bytes);
                    //display the data to the user
                    DisplayData(MessageType.Incoming, ByteToHex(comBuffer));
                    break;
                default:
                    //read data waiting in the buffer
                    string str = comPort.ReadExisting();
                    //display the data to the user
                    DisplayData(MessageType.Incoming, str);
                    break;
            }
        }
        #endregion
    }
}
