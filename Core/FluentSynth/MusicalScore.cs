namespace FluentSynth
{
    /// <summary>
    /// Plays a single beat, potentially as a chord
    /// </summary>
    /// <param name="Notes">A single MIDI note or a chord of notes</param>
    /// <param name="Duration">Duration: 1, 2, 4, 8, 16, 32</param>
    /// <param name="Velocity">The MIDI velocity range is from 0–127, with 127 being the loudest</param>
    public record Beat(int[] Notes, int Duration, int Velocity);
    /// <summary>
    /// Represents a measure for a specific instrument
    /// </summary>
    public sealed class MeasureSection
    {
        /// <summary>
        /// ID of MIDI instrument
        /// </summary>
        public int MIDIInstrument { get; set; } = Synth.AcousticGrandPiano;
        /// <summary>
        /// Number of beats, limited to size of measure
        /// </summary>
        public Beat[] Beats { get; set; }
    }
    /// <summary>
    /// Represents an N-beat measure, potentially have overlapping sections of different instruments
    /// </summary>
    public sealed class Measure
    {
        public MeasureSection[] Sections { get; set; }
    }
    /// <summary>
    /// At the moment we don't support arbitrary frequency, and no way to play two instruments simultaneously yet
    /// </summary>
    public sealed class Score
    {
        /// <summary>
        /// How many beats in a measure; Numerator in time signature,
        /// </summary>
        public int BeatsPerMeasure { get; set; } = 4;
        /// <summary>
        /// Beat size; denominator in time signature,
        /// </summary>
        public int BeatSize { get; set; } = 4;
        /// <summary>
        /// How many beats per minute, decides tempo.
        /// </summary>
        public int BPM { get; set; } = 4;
        /// <summary>
        /// Actual data stored as Measures.
        /// </summary>

        public Measure[] Measures;
    }

    public static class MusicalScoreParser
    {
        #region Mapping
        /// <summary>
        /// Mapping from human-readable names to MIDI note number
        /// </summary>
        public static Dictionary<string, int> NoteNameMapping { get; } = new()
        {
            // Special Beats
            { "_", -1 },

            // Simplified Names
            { "B", Synth.B4 },
            { "A#", Synth.As4 },
            { "Bb", Synth.Bb4 },
            { "A", Synth.A4 },
            { "G#", Synth.Gs4 },
            { "Ab", Synth.Ab4 },
            { "G", Synth.G4 },
            { "F#", Synth.Fs4 },
            { "Gb", Synth.Gb4 },
            { "F", Synth.F4 },
            { "E", Synth.E4 },
            { "D#", Synth.Ds4 },
            { "Eb", Synth.Eb4 },
            { "D", Synth.D4 },
            { "C#", Synth.Cs4 },
            { "Db", Synth.Db4 },
            { "C", Synth.C4 },

            // Fully Named
            { "C8", Synth.C8 },
            { "B7", Synth.B7 },
            { "A#7", Synth.As7 },
            { "Bb7", Synth.Bb7 },
            { "A7", Synth.A7 },
            { "G#7", Synth.Gs7 },
            { "Ab7", Synth.Ab7 },
            { "G7", Synth.G7 },
            { "F#7", Synth.Fs7 },
            { "Gb7", Synth.Gb7 },
            { "F7", Synth.F7 },
            { "E7", Synth.E7 },
            { "D#7", Synth.Ds7 },
            { "Eb7", Synth.Eb7 },
            { "D7", Synth.D7 },
            { "C#7", Synth.Cs7 },
            { "Db7", Synth.Db7 },
            { "C7", Synth.C7 },
            { "B6", Synth.B6 },
            { "A#6", Synth.As6 },
            { "Bb6", Synth.Bb6 },
            { "A6", Synth.A6 },
            { "G#6", Synth.Gs6 },
            { "Ab6", Synth.Ab6 },
            { "G6", Synth.G6 },
            { "F#6", Synth.Fs6 },
            { "Gb6", Synth.Gb6 },
            { "F6", Synth.F6 },
            { "E6", Synth.E6 },
            { "D#6", Synth.Ds6 },
            { "Eb6", Synth.Eb6 },
            { "D6", Synth.D6 },
            { "C#6", Synth.Cs6 },
            { "Db6", Synth.Db6 },
            { "C6", Synth.C6 },
            { "B5", Synth.B5 },
            { "A#5", Synth.As5 },
            { "Bb5", Synth.Bb5 },
            { "A5", Synth.A5 },
            { "G#5", Synth.Gs5 },
            { "Ab5", Synth.Ab5 },
            { "G5", Synth.G5 },
            { "F#5", Synth.Fs5 },
            { "Gb5", Synth.Gb5 },
            { "F5", Synth.F5 },
            { "E5", Synth.E5 },
            { "D#5", Synth.Ds5 },
            { "Eb5", Synth.Eb5 },
            { "D5", Synth.D5 },
            { "C#5", Synth.Cs5 },
            { "Db5", Synth.Db5 },
            { "C5", Synth.C5 },
            { "B4", Synth.B4 },
            { "A#4", Synth.As4 },
            { "Bb4", Synth.Bb4 },
            { "A4", Synth.A4 },
            { "ConcertPitch", Synth.ConcertPitch },
            { "G#4", Synth.Gs4 },
            { "Ab4", Synth.Ab4 },
            { "G4", Synth.G4 },
            { "F#4", Synth.Fs4 },
            { "Gb4", Synth.Gb4 },
            { "F4", Synth.F4 },
            { "E4", Synth.E4 },
            { "D#4", Synth.Ds4 },
            { "Eb4", Synth.Eb4 },
            { "D4", Synth.D4 },
            { "C#4", Synth.Cs4 },
            { "Db4", Synth.Db4 },
            { "C4", Synth.C4 },
            { "MiddleC", Synth.MiddleC },
            { "B3", Synth.B3 },
            { "A#3", Synth.As3 },
            { "Bb3", Synth.Bb3 },
            { "A3", Synth.A3 },
            { "G#3", Synth.Gs3 },
            { "Ab3", Synth.Ab3 },
            { "G3", Synth.G3 },
            { "F#3", Synth.Fs3 },
            { "Gb3", Synth.Gb3 },
            { "F3", Synth.F3 },
            { "E3", Synth.E3 },
            { "D#3", Synth.Ds3 },
            { "Eb3", Synth.Eb3 },
            { "D3", Synth.D3 },
            { "C#3", Synth.Cs3 },
            { "Db3", Synth.Db3 },
            { "C3", Synth.C3 },
            { "B2", Synth.B2 },
            { "A#2", Synth.As2 },
            { "Bb2", Synth.Bb2 },
            { "A2", Synth.A2 },
            { "G#2", Synth.Gs2 },
            { "Ab2", Synth.Ab2 },
            { "G2", Synth.G2 },
            { "F#2", Synth.Fs2 },
            { "Gb2", Synth.Gb2 },
            { "F2", Synth.F2 },
            { "E2", Synth.E2 },
            { "D#2", Synth.Ds2 },
            { "Eb2", Synth.Eb2 },
            { "D2", Synth.D2 },
            { "C#2", Synth.Cs2 },
            { "Db2", Synth.Db2 },
            { "C2", Synth.C2 },
            { "B1", Synth.B1 },
            { "A#1", Synth.As1 },
            { "Bb1", Synth.Bb1 },
            { "A1", Synth.A1 },
            { "G#1", Synth.Gs1 },
            { "Ab1", Synth.Ab1 },
            { "G1", Synth.G1 },
            { "F#1", Synth.Fs1 },
            { "Gb1", Synth.Gb1 },
            { "F1", Synth.F1 },
            { "E1", Synth.E1 },
            { "D#1", Synth.Ds1 },
            { "Eb1", Synth.Eb1 },
            { "D1", Synth.D1 },
            { "C#1", Synth.Cs1 },
            { "Db1", Synth.Db1 },
            { "C1", Synth.C1 },
            { "B0", Synth.B0 },
            { "A#0", Synth.As0 },
            { "Bb0", Synth.Bb0 },
            { "A0", Synth.A0 },
        };
        /// <summary>
        /// Mapping from human-readable names to MIDI instrument number
        /// </summary>
        public static Dictionary<string, int> InstrumentNameMapping { get; } = new()
        {
            // Standard Class
            { "Piano", Synth.AcousticGrandPiano },
            { "Chromatic Percussion", Synth.Celesta },
            { "Organ", Synth.HammondOrgan },
            { "Guitar", Synth.AcousticGuitar1 },
            { "Bass", Synth.AcousticBass },
            { "Strings", Synth.Violin },
            { "Ensemble", Synth.StringEnsemble1 },
            { "Brass", Synth.Trumpet },
            { "Reed", Synth.SopranoSax },
            { "Pipe", Synth.Piccolo },
            { "Synth Lead", Synth.Lead1 },
            { "Synth Pad", Synth.Pad1 },
            { "Synth Effects", Synth.FX1 },
            { "Ethnic", Synth.Sitar },
            { "Percussive", Synth.TinkleBell },
            { "Sound Effects", Synth.GuitarFretNoise },

            // Fully Named
            { "Acoustic Grand Piano", Synth.AcousticGrandPiano },
            { "Bright Acoustic Piano", Synth.BrightAcousticPiano },
            { "Electric Grand Piano", Synth.ElectricGrandPiano },
            { "Honky-tonk Piano", Synth.HonkytonkPiano },
            { "Rhodes Piano", Synth.RhodesPiano },
            { "Chorused Piano", Synth.ChorusedPiano },
            { "Harpsichord", Synth.Harpsichord },
            { "Clavinet", Synth.Clavinet },
            { "Celesta", Synth.Celesta },
            { "Glockenspiel", Synth.Glockenspiel },
            { "Music box", Synth.Musicbox },
            { "Vibraphone", Synth.Vibraphone },
            { "Marimba", Synth.Marimba },
            { "Xylophone", Synth.Xylophone },
            { "Tubular Bells", Synth.TubularBells },
            { "Dulcimer", Synth.Dulcimer },
            { "Hammond Organ", Synth.HammondOrgan },
            { "Percussive Organ", Synth.PercussiveOrgan },
            { "Rock Organ", Synth.RockOrgan },
            { "Church Organ", Synth.ChurchOrgan },
            { "Reed Organ", Synth.ReedOrgan },
            { "Accordion", Synth.Accordion },
            { "Harmonica", Synth.Harmonica },
            { "Tango Accordion", Synth.TangoAccordion },
            { "Acoustic Guitar 1", Synth.AcousticGuitar1 },
            { "Acoustic Guitar 2", Synth.AcousticGuitar2 },
            { "Electric Guitar 1", Synth.ElectricGuitar1 },
            { "Electric Guitar 2", Synth.ElectricGuitar2 },
            { "Electric Guitar 3", Synth.ElectricGuitar3 },
            { "Overdriven Guitar", Synth.OverdrivenGuitar },
            { "Distortion Guitar", Synth.DistortionGuitar },
            { "Guitar Harmonics", Synth.GuitarHarmonics },
            { "Acoustic Bass", Synth.AcousticBass },
            { "Electric Bass 1", Synth.ElectricBass1 },
            { "Electric Bass 2", Synth.ElectricBass2 },
            { "Fretless Bass", Synth.FretlessBass },
            { "Slap Bass 1", Synth.SlapBass1 },
            { "Slap Bass 2", Synth.SlapBass2 },
            { "Synth Bass 1", Synth.SynthBass1 },
            { "Synth Bass 2", Synth.SynthBass2 },
            { "Violin", Synth.Violin },
            { "Viola", Synth.Viola },
            { "Cello", Synth.Cello },
            { "Contrabass", Synth.Contrabass },
            { "Tremolo Strings", Synth.TremoloStrings },
            { "Pizzicato Strings", Synth.PizzicatoStrings },
            { "Orchestral Harp", Synth.OrchestralHarp },
            { "Timpani", Synth.Timpani },
            { "String Ensemble 1", Synth.StringEnsemble1 },
            { "String Ensemble 2", Synth.StringEnsemble2 },
            { "Synth Strings 1", Synth.SynthStrings1 },
            { "Synth Strings 2", Synth.SynthStrings2 },
            { "Choir Aahs", Synth.ChoirAahs },
            { "Voice Oohs", Synth.VoiceOohs },
            { "Synth Voice", Synth.SynthVoice },
            { "Orchestra Hit", Synth.OrchestraHit },
            { "Trumpet", Synth.Trumpet },
            { "Trombone", Synth.Trombone },
            { "Tuba", Synth.Tuba },
            { "Muted Trumpet", Synth.MutedTrumpet },
            { "French Horn", Synth.FrenchHorn },
            { "Brass Section", Synth.BrassSection },
            { "Synth Brass 1", Synth.SynthBrass1 },
            { "Synth Brass 2", Synth.SynthBrass2 },
            { "Soprano Sax", Synth.SopranoSax },
            { "Alto Sax", Synth.AltoSax },
            { "Tenor Sax", Synth.TenorSax },
            { "Baritone Sax", Synth.BaritoneSax },
            { "Oboe", Synth.Oboe },
            { "English Horn", Synth.EnglishHorn },
            { "Bassoon", Synth.Bassoon },
            { "Clarinet", Synth.Clarinet },
            { "Piccolo", Synth.Piccolo },
            { "Flute", Synth.Flute },
            { "Recorder", Synth.Recorder },
            { "Pan Flute", Synth.PanFlute },
            { "Bottle Blow", Synth.BottleBlow },
            { "Shakuhachi", Synth.Shakuhachi },
            { "Whistle", Synth.Whistle },
            { "Ocarina", Synth.Ocarina },
            { "Lead 1", Synth.Lead1 },
            { "Lead 2", Synth.Lead2 },
            { "Lead 3", Synth.Lead3 },
            { "Lead 4", Synth.Lead4 },
            { "Lead 5", Synth.Lead5 },
            { "Lead 6", Synth.Lead6 },
            { "Lead 7", Synth.Lead7 },
            { "Lead 8", Synth.Lead8 },
            { "Pad 1", Synth.Pad1 },
            { "Pad 2", Synth.Pad2 },
            { "Pad 3", Synth.Pad3 },
            { "Pad 4", Synth.Pad4 },
            { "Pad 5", Synth.Pad5 },
            { "Pad 6", Synth.Pad6 },
            { "Pad 7", Synth.Pad7 },
            { "Pad 8", Synth.Pad8 },
            { "FX 1", Synth.FX1 },
            { "FX 2", Synth.FX2 },
            { "FX 3", Synth.FX3 },
            { "FX 4", Synth.FX4 },
            { "FX 5", Synth.FX5 },
            { "FX 6", Synth.FX6 },
            { "FX 7", Synth.FX7 },
            { "FX 8", Synth.FX8 },
            { "Sitar", Synth.Sitar },
            { "Banjo", Synth.Banjo },
            { "Shamisen", Synth.Shamisen },
            { "Koto", Synth.Koto },
            { "Kalimba", Synth.Kalimba },
            { "Bagpipe", Synth.Bagpipe },
            { "Fiddle", Synth.Fiddle },
            { "Shana", Synth.Shana },
            { "Tinkle Bell", Synth.TinkleBell },
            { "Agogo", Synth.Agogo },
            { "Steel Drums", Synth.SteelDrums },
            { "Woodblock", Synth.Woodblock },
            { "Taiko Drum", Synth.TaikoDrum },
            { "Melodic Tom", Synth.MelodicTom },
            { "Synth Drum", Synth.SynthDrum },
            { "Reverse Cymbal", Synth.ReverseCymbal },
            { "Guitar Fret Noise", Synth.GuitarFretNoise },
            { "Breath Noise", Synth.BreathNoise },
            { "Seashore", Synth.Seashore },
            { "Bird Tweet", Synth.BirdTweet },
            { "Telephone Ring", Synth.TelephoneRing },
            { "Helicopter", Synth.Helicopter },
            { "Applause", Synth.Applause },
            { "Gunshot", Synth.Gunshot }
        };
        #endregion

        #region Methods
        /// <summary>
        /// Parse a script into score.
        /// </summary>
        public static Score Parse(string scoreScript)
        {
            Measure[] measures = scoreScript
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(measure => new Measure()
                {
                    Sections = new MeasureSection[]
                    {
                        new MeasureSection()
                        {
                            MIDIInstrument = Synth.AcousticGrandPiano,
                            Beats = measure.Trim().TrimStart('[').TrimEnd(']').Split(' ').Select(n => new Beat(new int[]{ NoteNameMapping[n] }, 4, 100)).ToArray()
                        }
                    }
                }).ToArray();
            return new Score()
            {
                BeatsPerMeasure = 4,
                BPM = 120,
                BeatSize = 4,
                Measures = measures,
            };
        }
        #endregion
    }
}
