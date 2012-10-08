using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Beeper
{
    public partial class PlayForm : Form
    {
        private Beeper beeper;

        private Dictionary<Keys, Note> keyMapping = new Dictionary<Keys, Note>();

        Action a;
        Random random = new Random();


        public PlayForm()
        {
            InitializeComponent();

            keyMapping.Add(Keys.A, Note.C);
            keyMapping.Add(Keys.S, Note.D);
            keyMapping.Add(Keys.D, Note.E);
            keyMapping.Add(Keys.F, Note.F);
            keyMapping.Add(Keys.G, Note.G);
            keyMapping.Add(Keys.H, Note.A);
            keyMapping.Add(Keys.J, Note.B);

            comboBox1.Items.AddRange(SerialPort.GetPortNames());

            beeper = new Beeper();
            // a = () => beeper.Play((uint)random.Next(100, 1000));
        }

        private byte next;

        private byte? IsAlreadyPlaying(uint freq)
        {
            for (byte i = 0; i < beeper.SpeakerCount; i++) {
                if (beeper[i] == freq) {
                    return i;
                }
            }

            return null;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Note note;
            if (keyMapping.TryGetValue(e.KeyCode, out note)) {
                ushort freq = (ushort)note;

                byte? index = IsAlreadyPlaying(freq);
                if (index == null) {
                    beeper.Play(next, freq);
                    next = (byte)((next + 1) % beeper.SpeakerCount);
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Note note;
            if (keyMapping.TryGetValue(e.KeyCode, out note)) {
                ushort freq = (ushort)note;

                byte? index = IsAlreadyPlaying(freq);
                if (index != null) {
                    beeper.Stop(index.Value);

                    Console.WriteLine("Stoping");
                    e.Handled = true;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisposeOldSerial();

            string portName = (string)comboBox1.SelectedItem;
            if (portName != null) {
                SerialPort serialPort = new SerialPort(portName, 9600);
                serialPort.Open();

                beeper.SerialPort = serialPort;
                comboBox1.Enabled = false;

                beeper.Handshake();
            }
        }

        private void DisposeOldSerial()
        {
            SerialPort oldSerial = beeper.SerialPort;
            if (oldSerial != null) {
                oldSerial.Dispose();
            }
        }

        private void PlayForm_Load(object sender, EventArgs e)
        {
            //Task task = new Task(a);
            //task.ContinueWith(RepeatAction);
            //task.Start();
        }

        void RepeatAction(Task task)
        {
            a();
            task.ContinueWith(RepeatAction);
        }
    }
}
