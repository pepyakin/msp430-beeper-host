using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beeper
{
    class MidiFrequency
    {

        private float[] midi = new float[127];

        private const float A = 440;

        public MidiFrequency()
        {
            float Aby32 = A / 32;

            for (int x = 0; x < 127; ++x)
            {
                midi[x] = Aby32 * (float)Math.Pow(2,  ((x - 9.0) / 12.0));
            }
        }

        public ushort this[int index]
        {
            get
            {
                return (ushort)midi[index];
            }
        }
    }
}
