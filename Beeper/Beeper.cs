using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Beeper
{
    class Beeper
    {
        public SerialPort SerialPort { get; set; }

        public byte SpeakerCount
        {
            get;
            private set;
        }

        private ushort[] freqs;
        private int next;

        public void Handshake()
        {
            byte[] buffer = new byte[] { 0x00 };
            SerialPort.Write(buffer, 0, 1);

            SpeakerCount = (byte) SerialPort.ReadByte();
            freqs = new ushort[SpeakerCount];
        }

        public void Play(byte ch, ushort freq)
        {
            Send(ch, freq);
        }

        public void Stop(byte ch)
        {
            Send(ch, 0);
        }

        private void Send(byte speaker, ushort freq)
        {
            if (SerialPort != null) {
                byte[] buffer = new byte[] { 0x01, speaker, (byte)(freq & 0xFF), (byte)((freq >> 8) & 0xFF) };

                for (int i = 0; i < buffer.Length; i++) {
                    SerialPort.Write(buffer, i, 1);
                }
                
                SerialPort.BaseStream.Flush();

                freqs[speaker] = freq;
            }
        }

        public ushort this[byte index]
        {
            get { return freqs[index]; }
            set { Play(index, value); }
        }
    }
}
