using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;
using System.Threading.Tasks;
using System.Threading;

namespace Beeper
{
    class MidiPlayer
    {
        MidiFile midiFile;
        private Beeper beeper;

        Semaphore semaphore;
        MidiFrequency frequency;

        public MidiPlayer(Beeper beeper, string fileName)
        {
            midiFile = new MidiFile(fileName);
            this.beeper = beeper;

            frequency = new MidiFrequency();
        }

        CountdownEvent e;

        public void Play()
        {
            int tracks = midiFile.Tracks;

            e = new CountdownEvent(tracks);
            Task[] tasks = new Task[tracks];

            for (int i = 0; i < tracks; i++) {
                int loc_i = i;
                tasks[i] = Task.Factory.StartNew(() => PlayTrack(loc_i));
                e.Signal();
            }

            Task.WaitAll(tasks);
        }

        private void PlayTrack(int track)
        {
            var events = midiFile.Events[track];

            track %= beeper.SpeakerCount;

            Action<String> log = (str) => {
                Console.WriteLine(track + ": " + str);
            };

            log("playing " + track);

            e.Wait();

            double tempo = 120;


            for (int i = 0; i < events.Count; i++) {
                MidiEvent ev = events[i];
                MidiEvent nextNote = null;

                if (i < events.Count - 1) {
                    nextNote = events[i + 1];
                }

                switch (ev.CommandCode) {
                    case MidiCommandCode.NoteOff:
                        NoteEvent noteEvent = (NoteEvent)ev;

                        beeper.Stop((byte)track);
                        log("Stop");
                        break;

                    case MidiCommandCode.NoteOn:
                        NoteOnEvent noteOn = (NoteOnEvent)ev;
                        ushort freq = frequency[noteOn.NoteNumber];
                        beeper.Play((byte)track, freq);

                        log(String.Format("Note on: {0}({1}), {2}", noteOn.NoteNumber, freq, noteOn.NoteLength));
                        break;

                    case MidiCommandCode.MetaEvent:
                        MetaEvent metaEvent = (MetaEvent)ev;

                        if (metaEvent.MetaEventType == MetaEventType.EndTrack) {
                            beeper.Stop((byte)track);
                            return;
                        }

                        if (ev is TempoEvent) {
                            TempoEvent tempoEvent = (TempoEvent)metaEvent;
                            tempo = tempoEvent.Tempo;
                        }
                        
                        break;

                    default:
                        tempo = 128;

                        break;
                }

                if (nextNote.DeltaTime > 0) {
                    Thread.Sleep(nextNote.DeltaTime);
                }
            }
        }
    }
}
