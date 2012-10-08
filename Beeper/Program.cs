using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Beeper
{
    static class Program
    {
        static int tx_bytes = 0;
        static int rx_bytes = 0;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new PlayForm());

            Beeper beeper = new Beeper();
            beeper.SerialPort = new SerialPort("COM6", 9600);
            beeper.SerialPort.Open();
            Console.WriteLine("opened!");

            beeper.Handshake();
            Console.WriteLine("handsheked: speakers " + beeper.SpeakerCount);


            MidiPlayer player = new MidiPlayer(beeper, "imperial.mid");
            player.Play();

            while (true) {
                Thread.Yield();
            }

            // RunTests();
        }

        private static void RunTests()
        {
            SerialPort port = new SerialPort("COM6", 9600);
            port.Open();

            const int count = 2;

            port.ErrorReceived += (s, e) => {
                Console.WriteLine("Error: " + e.EventType);
            };

            while (true) {
                Random random = new Random();
                byte[] buffer = new byte[count];

                buffer[0] = (byte)random.Next(255);
                buffer[1] = (byte)random.Next(255);

                port.Write(buffer, 0, count);
                tx_bytes += 1;


                byte[] input = new byte[count];

                int readed = 0;
                for (int i = 0; i < count; i++) {
                    input[i] = (byte)port.ReadByte();
                    rx_bytes++;
                }

                rx_bytes += readed;

                Console.WriteLine("{0} - expected: {1}, received: {2}", rx_bytes, buffer, input);

                for (int i = 0; i < count; i++) {
                    if (buffer[i] != input[i]) {
                        Console.WriteLine("miss: {0} != {1}", buffer[i], input[i]);
                    }
                }
            }
        }
    }
}
