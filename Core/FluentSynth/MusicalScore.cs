using System.Text.RegularExpressions;

namespace FluentSynth
{
    /// <summary>
    /// Represent a pitch/sound of a note; Can be either a musical pitch or a vocal.
    /// </summary>
    public record NotePitch(int Pitch, string VocalName)
    {
        /// <summary>
        /// Whether this pitch represents a vocal;
        /// When true its pitch value is not used (and assigned as Synth.StopNote).
        /// </summary>
        public bool IsVocal => VocalName != null;
    }
    /// <summary>
    /// Plays a single note, potentially as a chord
    /// </summary>
    /// <param name="Pitches">A single MIDI note or a chord of notes, or a vocal</param>
    /// <param name="Duration">Duration: 1, 2, 4, 8, 16, 32</param>
    /// <param name="Velocity">The MIDI velocity range is from 0–127, with 127 being the loudest</param>
    /// <param name="ExtendedDuration">How many additional 1/2 durations should we extend from Duration of this note</param>
    public record Note(NotePitch[] Pitches, int Duration, int Velocity, int ExtendedDuration)
    {
        /// <summary>
        /// How many beats is this note
        /// </summary>
        public double GetBeatCount(int beatSize)
        {
            return ExtendedDuration > 0 
                ? (double) beatSize * (1 + 0.5 * ExtendedDuration) / Duration
                : (double) beatSize / Duration;
        }
    }

    /// <summary>
    /// Represents a measure for a specific instrument;
    /// This corresponds to a specific instrument channel from MIDI's perspective.
    /// Each song is allowed to have 16 unique channels.
    /// (Anything more than that might require mixing using NAudio on the waveform level)
    /// </summary>
    public sealed class MeasureSection
    {
        /// <summary>
        /// Name of the section in terms of instrument group;
        /// Equivalent to MIDI channel;
        /// Equivalent to DAW track.
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// ID of MIDI instrument
        /// </summary>
        public int MIDIInstrument { get; set; } = Synth.AcousticGrandPiano;
        /// <summary>
        /// Number of notes, limited to size of measure
        /// </summary>
        public Note[] Notes { get; set; }

        /// <summary>
        /// Calculates how many units of beats we actually have in this measure.
        /// Units of beats can be fractions.
        /// For instance, in a 4/4, a quarter note is 1 unit, an eighth note is 1/2 a unit, while a whole note is 4 units.
        /// </summary>
        /// <param name="beatSize">Beat size per time signature.</param>
        /// <returns>Number of actual beat units</returns>
        /// <remarks>
        /// Notice because of the way that music scores are typically assigned and the existence of quarter/eighth etc. notes, the numbers of "notes" in a measure doesn't necessarily match the number of beats.
        /// </remarks>
        public double GetBeatCount(int beatSize) => Notes.Sum(n => n.GetBeatCount(beatSize));
    }
    /// <summary>
    /// Represents an N-note measure, potentially have overlapping sections of different instruments
    /// </summary>
    public sealed class Measure
    {
        /// <summary>
        /// Sections represent the "channels" or "tracks" as divided into measures.
        /// This can be used to represent different instruments on a score;
        /// Or different tracks in a DAW application.
        /// Each instrument or instrument group takes up one section and are all collected inside a particular measure, which divides the total time (duration) of the musical piece (score) in Fluent Synth.
        /// </summary>
        /// <remarks>
        /// A channel is an independent path over which messages travel to their destination. 
        /// There are 16 channels per MIDI device. A track in your sequencer program plays one instrument over a single channel. The MIDI messages in the track find their way to the instrument over that channel.
        /// </remarks>
        public MeasureSection[] Sections { get; set; }
    }
    /// <summary>
    /// At the moment we don't support arbitrary frequency, and no way to play two instruments simultaneously yet
    /// </summary>
    public sealed class Score
    {
        #region Properties
        /// <summary>
        /// How many beats in a measure; Numerator in time signature,
        /// </summary>
        public int BeatsPerMeasure { get; set; } = 4;
        /// <summary>
        /// Beat size for a unit bit; denominator in time signature, Actual beat count is beat size divided by note duration (1, 2, 4, 8 etc.).
        /// E.g. A beat size of 4, with a note duration of 8, gives an actual units of beat of 4/8=1/2 length, or half a beat count.
        /// </summary>
        public int BeatSize { get; set; } = 4;
        /// <summary>
        /// How many beats per minute, decides tempo.
        /// </summary>
        public int BPM { get; set; } = 4;
        #endregion

        #region Data
        /// <summary>
        /// A list of vocals referencing WAV files and function like notes
        /// </summary>
        public Dictionary<string, string> Vocals { get; set; }
        /// <summary>
        /// Actual data stored as Measures.
        /// </summary>
        public Measure[] Measures;
        #endregion

        #region Accessors
        /// <summary>
        /// Duration of a measure in seconds
        /// </summary>
        public double MeasureSizeInSeconds => (double) BeatsPerMeasure / BPM * 60;
        /// <summary>
        /// Total duration of the entire score
        /// </summary>
        public double TotalSeconds => Math.Ceiling(Measures.Length * MeasureSizeInSeconds);
        /// <summary>
        /// Total number of samples as determined by sample rate throughout the duration of the music piece.
        /// </summary>
        /// <remarks>
        /// TODO: May have alignment problem
        /// </remarks>
        public int GetTotalSamples(int sampleRate) => (int)(TotalSeconds * sampleRate);
        /// <summary>
        /// Get how many samples is required for each measure.
        /// </summary>
        /// <remarks>
        /// TODO: May have alignment problem
        /// </remarks>
        public int GetMeasureSizeInFloats(int sampleRate) => (int)(MeasureSizeInSeconds * sampleRate);
        /// <summary>
        /// Get how many samples is required for a bit as deteremined by sample rate.
        /// </summary>
        public int GetBeatSizeInFloats(int sampleRate) => GetMeasureSizeInFloats(sampleRate) / BeatsPerMeasure;
        #endregion
    }

    /// <summary>
    /// Provides parsing for musical score written in plain text;
    /// The format is roughly a modified version of Guido Music Notation.
    /// </summary>
    public static partial class MusicalScoreParser
    {
        #region Constants
        /// <summary>
        /// Starting line for multi-instrument scores
        /// </summary>
        public static readonly string MultiInstrumentModeToggle = "Mode: Multi-Instrument";
        #endregion

        #region Mapping
        /// <summary>
        /// Mapping from human-readable names to MIDI note number
        /// </summary>
        public static Dictionary<string, int> NoteNameMapping { get; } = new()
        {
            // Special Beats
            { "_", Synth.StopNote },
            { "-", Synth.StopNote },

            // Simplified Names
            { "A'", Synth.A5 },
            { "B'", Synth.B5 },
            { "C'", Synth.C5 },
            { "D'", Synth.D5 },
            { "E'", Synth.E5 },
            { "F'", Synth.F5 },
            { "G'", Synth.G5 },
            { "A_", Synth.A3 },
            { "B_", Synth.B3 },
            { "C_", Synth.C3 },
            { "D_", Synth.D3 },
            { "E_", Synth.E3 },
            { "F_", Synth.F3 },
            { "G_", Synth.G3 },
            { "B", Synth.B4 },
            { "A#", Synth.As4 },
            { "B&", Synth.Bb4 },
            { "A", Synth.A4 },
            { "G#", Synth.Gs4 },
            { "A&", Synth.Ab4 },
            { "G", Synth.G4 },
            { "F#", Synth.Fs4 },
            { "G&", Synth.Gb4 },
            { "F", Synth.F4 },
            { "E", Synth.E4 },
            { "D#", Synth.Ds4 },
            { "E&", Synth.Eb4 },
            { "D", Synth.D4 },
            { "C#", Synth.Cs4 },
            { "D&", Synth.Db4 },
            { "C", Synth.C4 },

            // Solfège in the key of C, spanning three octaves
            { "do", Synth.C4 },
            { "re", Synth.D4 },
            { "mi", Synth.E4 },
            { "fa", Synth.F4 },
            { "sol", Synth.G4 },
            { "la", Synth.A4 },
            { "ti", Synth.B4 },
            { "si", Synth.B4 },
            { "do'", Synth.C5 },
            { "re'", Synth.D5 },
            { "mi'", Synth.E5 },
            { "fa'", Synth.F5 },
            { "sol'", Synth.G5 },
            { "la'", Synth.A5 },
            { "ti'", Synth.B5 },
            { "si'", Synth.B5 },
            { "do_", Synth.C3 },
            { "re_", Synth.D3 },
            { "mi_", Synth.E3 },
            { "fa_", Synth.F3 },
            { "sol_", Synth.G3 },
            { "la_", Synth.A3 },
            { "ti_", Synth.B3 },
            { "si_", Synth.B3 },

            // Number notations in the key of C, spanning three octaves
            { "1", Synth.C4 },
            { "2", Synth.D4 },
            { "3", Synth.E4 },
            { "4", Synth.F4 },
            { "5", Synth.G4 },
            { "6", Synth.A4 },
            { "7", Synth.B4 },
            { "1'", Synth.C5 },
            { "2'", Synth.D5 },
            { "3'", Synth.E5 },
            { "4'", Synth.F5 },
            { "5'", Synth.G5 },
            { "6'", Synth.A5 },
            { "7'", Synth.B5 },
            { "1_", Synth.C3 },
            { "2_", Synth.D3 },
            { "3_", Synth.E3 },
            { "4_", Synth.F3 },
            { "5_", Synth.G3 },
            { "6_", Synth.A3 },
            { "7_", Synth.B3 },

            // Fully Named
            { "C8", Synth.C8 },
            { "B7", Synth.B7 },
            { "A#7", Synth.As7 },
            { "B&7", Synth.Bb7 },
            { "A7", Synth.A7 },
            { "G#7", Synth.Gs7 },
            { "A&7", Synth.Ab7 },
            { "G7", Synth.G7 },
            { "F#7", Synth.Fs7 },
            { "G&7", Synth.Gb7 },
            { "F7", Synth.F7 },
            { "E7", Synth.E7 },
            { "D#7", Synth.Ds7 },
            { "E&7", Synth.Eb7 },
            { "D7", Synth.D7 },
            { "C#7", Synth.Cs7 },
            { "D&7", Synth.Db7 },
            { "C7", Synth.C7 },
            { "B6", Synth.B6 },
            { "A#6", Synth.As6 },
            { "B&6", Synth.Bb6 },
            { "A6", Synth.A6 },
            { "G#6", Synth.Gs6 },
            { "A&6", Synth.Ab6 },
            { "G6", Synth.G6 },
            { "F#6", Synth.Fs6 },
            { "G&6", Synth.Gb6 },
            { "F6", Synth.F6 },
            { "E6", Synth.E6 },
            { "D#6", Synth.Ds6 },
            { "E&6", Synth.Eb6 },
            { "D6", Synth.D6 },
            { "C#6", Synth.Cs6 },
            { "D&6", Synth.Db6 },
            { "C6", Synth.C6 },
            { "B5", Synth.B5 },
            { "A#5", Synth.As5 },
            { "B&5", Synth.Bb5 },
            { "A5", Synth.A5 },
            { "G#5", Synth.Gs5 },
            { "A&5", Synth.Ab5 },
            { "G5", Synth.G5 },
            { "F#5", Synth.Fs5 },
            { "G&5", Synth.Gb5 },
            { "F5", Synth.F5 },
            { "E5", Synth.E5 },
            { "D#5", Synth.Ds5 },
            { "E&5", Synth.Eb5 },
            { "D5", Synth.D5 },
            { "C#5", Synth.Cs5 },
            { "D&5", Synth.Db5 },
            { "C5", Synth.C5 },
            { "B4", Synth.B4 },
            { "A#4", Synth.As4 },
            { "B&4", Synth.Bb4 },
            { "A4", Synth.A4 },
            { "ConcertPitch", Synth.ConcertPitch },
            { "G#4", Synth.Gs4 },
            { "A&4", Synth.Ab4 },
            { "G4", Synth.G4 },
            { "F#4", Synth.Fs4 },
            { "G&4", Synth.Gb4 },
            { "F4", Synth.F4 },
            { "E4", Synth.E4 },
            { "D#4", Synth.Ds4 },
            { "E&4", Synth.Eb4 },
            { "D4", Synth.D4 },
            { "C#4", Synth.Cs4 },
            { "D&4", Synth.Db4 },
            { "C4", Synth.C4 },
            { "MiddleC", Synth.MiddleC },
            { "B3", Synth.B3 },
            { "A#3", Synth.As3 },
            { "B&3", Synth.Bb3 },
            { "A3", Synth.A3 },
            { "G#3", Synth.Gs3 },
            { "A&3", Synth.Ab3 },
            { "G3", Synth.G3 },
            { "F#3", Synth.Fs3 },
            { "G&3", Synth.Gb3 },
            { "F3", Synth.F3 },
            { "E3", Synth.E3 },
            { "D#3", Synth.Ds3 },
            { "E&3", Synth.Eb3 },
            { "D3", Synth.D3 },
            { "C#3", Synth.Cs3 },
            { "D&3", Synth.Db3 },
            { "C3", Synth.C3 },
            { "B2", Synth.B2 },
            { "A#2", Synth.As2 },
            { "B&2", Synth.Bb2 },
            { "A2", Synth.A2 },
            { "G#2", Synth.Gs2 },
            { "A&2", Synth.Ab2 },
            { "G2", Synth.G2 },
            { "F#2", Synth.Fs2 },
            { "G&2", Synth.Gb2 },
            { "F2", Synth.F2 },
            { "E2", Synth.E2 },
            { "D#2", Synth.Ds2 },
            { "E&2", Synth.Eb2 },
            { "D2", Synth.D2 },
            { "C#2", Synth.Cs2 },
            { "D&2", Synth.Db2 },
            { "C2", Synth.C2 },
            { "B1", Synth.B1 },
            { "A#1", Synth.As1 },
            { "B&1", Synth.Bb1 },
            { "A1", Synth.A1 },
            { "G#1", Synth.Gs1 },
            { "A&1", Synth.Ab1 },
            { "G1", Synth.G1 },
            { "F#1", Synth.Fs1 },
            { "G&1", Synth.Gb1 },
            { "F1", Synth.F1 },
            { "E1", Synth.E1 },
            { "D#1", Synth.Ds1 },
            { "E&1", Synth.Eb1 },
            { "D1", Synth.D1 },
            { "C#1", Synth.Cs1 },
            { "D&1", Synth.Db1 },
            { "C1", Synth.C1 },
            { "B0", Synth.B0 },
            { "A#0", Synth.As0 },
            { "B&0", Synth.Bb0 },
            { "A0", Synth.A0 },
        };
        /// <summary>
        /// Mapping from human-readable names to MIDI instrument number
        /// </summary>
        public static Dictionary<string, int> InstrumentNameMapping { get; } = new()
        {
            // Special track
            { "Vocal", Synth.Vocal },

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
            { "Music Box", Synth.MusicBox },
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
            { "Acoustic Guitar (nylon)", Synth.AcousticGuitar1 },
            { "Acoustic Guitar (steel)", Synth.AcousticGuitar2 },
            { "Electric Guitar (jazz)", Synth.ElectricGuitar1 },
            { "Electric Guitar (clean)", Synth.ElectricGuitar2 },
            { "Electric Guitar (muted)", Synth.ElectricGuitar3 },
            { "Overdriven Guitar", Synth.OverdrivenGuitar },
            { "Distortion Guitar", Synth.DistortionGuitar },
            { "Guitar Harmonics", Synth.GuitarHarmonics },
            { "Acoustic Bass", Synth.AcousticBass },
            { "Electric Bass (finger)", Synth.ElectricBass1 },
            { "Electric Bass (pick)", Synth.ElectricBass2 },
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
            { "Lead 1 (square)", Synth.Lead1 },
            { "Lead 2 (sawtooth)", Synth.Lead2 },
            { "Lead 3 (calliope lead)", Synth.Lead3 },
            { "Lead 4 (chiffer lead)", Synth.Lead4 },
            { "Lead 5 (charang)", Synth.Lead5 },
            { "Lead 6 (voice)", Synth.Lead6 },
            { "Lead 7 (fifths)", Synth.Lead7 },
            { "Lead 8 (brass + lead)", Synth.Lead8 },
            { "Pad 1 (new age)", Synth.Pad1 },
            { "Pad 2 (warm)", Synth.Pad2 },
            { "Pad 3 (polysynth)", Synth.Pad3 },
            { "Pad 4 (choir)", Synth.Pad4 },
            { "Pad 5 (bowed)", Synth.Pad5 },
            { "Pad 6 (metallic)", Synth.Pad6 },
            { "Pad 7 (halo)", Synth.Pad7 },
            { "Pad 8 (sweep)", Synth.Pad8 },
            { "FX 1 (rain)", Synth.FX1 },
            { "FX 2 (soundtrack)", Synth.FX2 },
            { "FX 3 (crystal)", Synth.FX3 },
            { "FX 4 (atmosphere)", Synth.FX4 },
            { "FX 5 (brightness)", Synth.FX5 },
            { "FX 6 (goblins)", Synth.FX6 },
            { "FX 7 (echoes)", Synth.FX7 },
            { "FX 8 (sci-fi)", Synth.FX8 },
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
            // Loose notes
            if (!scoreScript.Contains('[') && !scoreScript.Contains(']'))
                return ParseLooseNotes(scoreScript);
            // Complete composition divided in measures
            else if (scoreScript.Trim().StartsWith(MultiInstrumentModeToggle) 
                || Regex.IsMatch(scoreScript, "^.*?:", RegexOptions.Multiline)) // Heuristic intended multi-instrument mode
                return ParseCompleteScoreMultipleInstruments(scoreScript);
            else
                return ParseCompleteScoreSingleInstrument(scoreScript);
        }
        /// <summary>
        /// Parses score for multi-instrument mode.
        /// </summary>
        public static Score ParseCompleteScoreMultipleInstruments(string scoreScript)
        {
            string[] contentLines = SplitScriptLines(scoreScript)
                .Skip(scoreScript.Contains(MultiInstrumentModeToggle) ? 1 : 0)
                .ToArray();

            // Parse time signature
            ParseTimeSignature(contentLines.First(), out int beatsPerMeasure, out int beatSize, out int tempo);

            // Parse vocals
            IEnumerable<string> vocalLines = contentLines.Where(line => ScoreVocalDefinitionLineRegex().IsMatch(line));
            Dictionary<string, string> vocals = vocalLines
                .Select(line => ScoreVocalDefinitionLineRegex().Match(line))
                .Select(match => (Key: match.Groups[1].Value, File: match.Groups[2].Value))
                .ToDictionary(p => p.Key, p => p.File);

            // Parse measure content lines
            (string Line, int Index, (string Grouped, string Instrument) GroupedInstrument, bool IsValid, Measure[] Measures)[] lineMeasures = contentLines
                .Skip(1)    // Skip time sianguare
                .Where(line => !ScoreVocalDefinitionLineRegex().IsMatch(line))
                .Select((line, index) => {
                    Match groupedInstrument = ScoreMultiInstrumentLineGroupedInstrumentRegex().Match(line);
                    string instrumentName = groupedInstrument.Groups[3].Value.Trim();
                    string groupName = groupedInstrument.Groups[2].Value.Trim();
                    if (string.IsNullOrEmpty(groupName)) groupName = instrumentName;

                    return (
                        Line: line,
                        Index: index,
                        GroupedInstrument: groupedInstrument.Success 
                            ? (GroupName: groupName, InstrumentName: instrumentName) 
                            : (null, null), // Named instrument group, syntax `Group Nam:Instrument Name`
                        IsValid: groupedInstrument.Success,
                        Measures: ScoreMeasureRegex()
                            .Matches(line)
                            .Select(m => m.Value)
                            .Select(measure => CreateMeasure(measure, beatsPerMeasure, beatSize, vocals))
                            .ToArray()
                    );
                })
                .ToArray();

            // Validate lines
            if (lineMeasures.Any(l => !l.IsValid))
                throw new ArgumentException($"Invalid line - missing grouped:instrument: {lineMeasures.First(l => l.IsValid)}");

            // Assign section instruments
            Dictionary<(string GroupName, string InstrumentName), Measure[]> instrumentGroups = lineMeasures
                .GroupBy(m => m.GroupedInstrument)
                .Select(g => (InstrumentGroup: g.Key, Measures: g.SelectMany(g => g.Measures).ToArray()))
                .ToDictionary(g => g.InstrumentGroup, g => g.Measures);
            foreach (var group in instrumentGroups)
            {
                foreach (var measure in group.Value)
                {
                    foreach (var section in measure.Sections)
                    {
                        section.MIDIInstrument = InstrumentNameMapping[group.Key.InstrumentName];
                        section.GroupName = group.Key.GroupName;
                    }
                }
            }

            // Assemble/zip measures
            int maxUniqueSections = instrumentGroups.Max(g => g.Value.Count());
            Measure[] assemble = new Measure[maxUniqueSections];
            for (int i = 0; i < maxUniqueSections; i++)
            {
                assemble[i] = new Measure()
                {
                    Sections = instrumentGroups
                        .Where(g => g.Value.Length > i)
                        .Select(g => g.Value[i])
                        .Select(m => m.Sections.Single())
                        .ToArray()
                };
            }

            return new Score()
            {
                BeatsPerMeasure = beatsPerMeasure,
                BPM = tempo,
                BeatSize = beatSize,
                Measures = assemble.ToArray(),
                Vocals = vocals
            };
        }
        private static Score ParseCompleteScoreSingleInstrument(string scoreScript)
        {
            scoreScript = RemoveCommentLines(scoreScript);
            scoreScript = ParseTimeSignature(scoreScript, out int beatsPerMeasure, out int beatSize, out int tempo).Trim();

            Measure[] measures = SplitScriptLines(scoreScript)
                .SelectMany(line => ScoreMeasureRegex().Matches(line).Select(m => m.Value))
                .Select(measure => CreateMeasure(measure, beatsPerMeasure, beatSize))
                .ToArray();
            return new Score()
            {
                BeatsPerMeasure = beatsPerMeasure,
                BPM = tempo,
                BeatSize = beatSize,
                Measures = measures,
            };
        }

        private static Score ParseLooseNotes(string scoreScript)
        {
            scoreScript = RemoveCommentLines(scoreScript);
            scoreScript = ParseTimeSignature(scoreScript, out int beatsPerMeasure, out int beatSize, out int tempo);
            List<Measure> measures = new();

            foreach (var line in SplitScriptLines(scoreScript))
            {
                string remainingLine = ParseDefaultInstrument(line, out string defaultInstrument);

                double currentBeats = 0;
                List<Note> currentNotes = new();
                var notes = remainingLine
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(note => (Symbol: note, Note: CreateNote(note)))
                    .ToArray();
                for (int i = 0; i < notes.Length; i++)
                {
                    (string Symbol, Note Note) note = notes[i];

                    double noteBeatCount = note.Note.GetBeatCount(beatSize);
                    if (currentBeats + noteBeatCount > beatsPerMeasure)
                        throw new ArgumentException($"Beats duration exceeds measure: {note.Symbol} @{i} ({(i > 0 ? $"after {notes[i-1].Symbol}" : "at the beginning")})");
                    else
                    {
                        currentNotes.Add(note.Note);
                        currentBeats += noteBeatCount;
                    }

                    if (currentBeats == beatsPerMeasure)
                    {
                        AddNotes(measures, currentNotes, defaultInstrument);
                        currentBeats = 0;
                    }
                }
                if (currentNotes.Count != 0)
                    AddNotes(measures, currentNotes, defaultInstrument);
            }

            return new Score()
            {
                BeatsPerMeasure = beatsPerMeasure,
                BPM = tempo,
                BeatSize = beatSize,
                Measures = measures.ToArray(),
            };

            static string ParseDefaultInstrument(string scoreScript, out string defaultInstrument)
            {
                defaultInstrument = "Piano";
                Match instrumentDefinition = Regex.Match(scoreScript, @"^{(.*?)}");
                if (instrumentDefinition.Success)
                {
                    defaultInstrument = instrumentDefinition.Groups[1].Value;
                    scoreScript = scoreScript[instrumentDefinition.Length..];
                }
                // Pre-emptively guess intention of assigning default instrument even if the syntax is not `{Instrument Name}` at the start of line
                else if (InstrumentNameMapping.ContainsKey(scoreScript.Split(' ').First()))
                {
                    defaultInstrument = scoreScript.Split(' ').First();
                    scoreScript = scoreScript[defaultInstrument.Length..].TrimStart();
                }

                return scoreScript;
            }
            static void AddNotes(List<Measure> measures, List<Note> currentNotes, string defaultInstrument)
            {
                measures.Add(new Measure()
                {
                    Sections = new MeasureSection[]
                    {
                        new MeasureSection()
                        {
                            MIDIInstrument = InstrumentNameMapping[defaultInstrument],
                            Notes = currentNotes.ToArray()
                        }
                    }
                });

                currentNotes.Clear();
            }
        }
        private static string ParseTimeSignature(string scoreScript, out int beatsPerMeasure, out int beatSize, out int tempo)
        {
            beatsPerMeasure = 4;
            beatSize = 4;
            tempo = 120;
            scoreScript = scoreScript.Trim();
            Match tempoDefinition = Regex.Match(scoreScript, @"^\((\d+)\)");
            if (tempoDefinition.Success)
            {
                tempo = int.Parse(tempoDefinition.Groups[1].Value);

                scoreScript = scoreScript[tempoDefinition.Length..];
            }

            scoreScript = scoreScript.Trim();
            Match timeSignatureDefinition = Regex.Match(scoreScript, @"^(\d+)/(\d+)");
            if (timeSignatureDefinition.Success)
            {
                beatsPerMeasure = int.Parse(timeSignatureDefinition.Groups[1].Value);
                beatSize = int.Parse(timeSignatureDefinition.Groups[2].Value);

                scoreScript = scoreScript[timeSignatureDefinition.Length..];
            }

            return scoreScript.Trim();
        }
        #endregion

        #region Routines
        /// <summary>
        /// Create a measure from measure notation
        /// </summary>
        public static Measure CreateMeasure(string script, int beatsPerMeasure, int beatSize, Dictionary<string, string> vocals = null)
        {
            Match match = ScoreMeasureRegex().Match(script);
            string instrument = match.Groups[2].Value;
            string section = match.Groups[3].Value;

            Measure measure = new()
            {
                Sections = new MeasureSection[]
                {
                    new MeasureSection()
                    {
                        MIDIInstrument = string.IsNullOrEmpty(instrument) ? Synth.AcousticGrandPiano : InstrumentNameMapping[instrument],
                        Notes = section.Split(' ').Select(n => CreateNote(n, vocals)).ToArray()
                    }
                }
            };

            MeasureSection invalidSection = measure.Sections.FirstOrDefault(s => s.GetBeatCount(beatSize) != beatsPerMeasure);
            if (invalidSection != null)
                throw new ArgumentException($"Invalid number of beats: {script}; Should be {beatsPerMeasure} beats per measure - got {invalidSection.GetBeatCount(beatSize)} beats.");

            return measure;
        }
        /// <summary>
        /// Create a beat from note notation
        /// </summary>
        public static Note CreateNote(string note, Dictionary<string, string> vocals = null)
        {
            int defaultDuration = 4;
            int defaultAttack = 100;

            Match match = ScoreMeasureMusicalNoteWithDurationAndAttackRegex().Match(note);
            if (match.Success)
            {
                string notesString = match.Groups[1].Value;
                string durationString = match.Groups[3].Value;
                int extendedDuration = match.Groups[4].Value.Length;
                string attackString = match.Groups[6].Value;

                NotePitch[] pitches = notesString
                    .Split('|')
                    .Select(pitchName => {
                        if (vocals != null && vocals.ContainsKey(pitchName))
                            return new NotePitch(Synth.VocalNote, pitchName);
                        else
                        {
                            if (!NoteNameMapping.ContainsKey(pitchName.ToUpper()))
                               throw new ArgumentException($"Note name `{pitchName}` is invalid.");
                            else return new NotePitch(NoteNameMapping[pitchName.ToUpper()], null);
                        }
                    })
                    .ToArray();
                return new Note(
                    pitches,
                    string.IsNullOrEmpty(durationString) ? defaultDuration : int.Parse(durationString),
                    string.IsNullOrEmpty(attackString) ? defaultAttack : int.Parse(attackString),
                    extendedDuration
                );
            }
            else
                throw new ArgumentException($"Canot parse note: {note}");
        }
        #endregion

        #region Helpers
        private static string RemoveCommentLines(string scoreScript)
        {
            return String.Join(Environment.NewLine, SplitScriptLines(scoreScript));
        }
        private static IEnumerable<string> SplitScriptLines(string scoreScript)
        {
            return scoreScript
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.TrimStart().StartsWith('#'));
        }
        #endregion

        #region Regular Expressions
        [GeneratedRegex(@"({([a-zA-Z ]+)})?\s*\[(.*?)\]")]
        private static partial Regex ScoreMeasureRegex();
        [GeneratedRegex(@"^(([^:]*?):)?([^\[]*)\s*")]
        private static partial Regex ScoreMultiInstrumentLineGroupedInstrumentRegex();
        [GeneratedRegex(@"^([a-zA-Z0-9_$]*): (.*)$")]
        private static partial Regex ScoreVocalDefinitionLineRegex();
        [GeneratedRegex(@"^(.*?)(/(\d+))?(\.*)(@(\d+))?$")]
        private static partial Regex ScoreMeasureMusicalNoteWithDurationAndAttackRegex();
        #endregion
    }
}
