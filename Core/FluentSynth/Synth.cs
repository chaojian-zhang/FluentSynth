using MeltySynth;
using NAudio.Wave;

namespace FluentSynth
{
    /// <summary>
    /// Main class exposing a wide variety of functionalities related to frequency-based synth, MIDI synth, and various constants.
    /// </summary>
    public sealed class Synth
    {
        #region Properties
        /// <summary>
        /// Path of the sound font file for MIDI synth
        /// </summary>
        public string SoundFontFilePath { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize synth with specific sound file path;
        /// Can be null if MIDI related functionalities are not used.
        /// </summary>
        public Synth(string sounfFontFilePath = null)
        {
            SoundFontFilePath = sounfFontFilePath;
        }
        #endregion

        #region Commonly Used Constants - Sample Frequencies
        /// <summary>
        /// Bit rate of PCM waveform
        /// </summary>
        public const int BitsPerSample = sizeof(short) * 8; // 16
        /// <summary>
        /// Provides common reference sample rate: 44100
        /// </summary>
        public const int SampleRate44K = 44100;
        /// <summary>
        /// Default sample rate used by most functions
        /// </summary>
        public const int DefaultSampleRate = SampleRate44K;
        #endregion

        #region Commonly Used Constants - MIDI Note Names
        /// <summary>
        /// Represents a stop.
        /// This is not a MIDI note number but used internally by Orchestrator to represent stops.
        /// </summary>
        public const int StopNote = -1;
        /// <summary>
        /// Represents a vocal note.
        /// This is not a MIDI note number but used internally by Orchestrator to represent vocals or audio samples.
        /// </summary>
        public const int VocalNote = -2;

        /// <summary>
        /// MIDI C8 (108)
        /// Piano key number: 88
        /// Frequency: 4,186.01 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C8 = 108;
        /// <summary>
        /// MIDI B7 (107)
        /// Piano key number: 87
        /// Frequency: 3,951.07 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B7 = 107;
        /// <summary>
        /// MIDI A#7/Bb7 (106)
        /// Piano key number: 86
        /// Frequency: 3,729.31 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As7 = 106;
        /// <summary>
        /// MIDI A#7/Bb7 (106)
        /// Piano key number: 86
        /// Frequency: 3,729.31 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb7 = 106;
        /// <summary>
        /// MIDI A7 (105)
        /// Piano key number: 85
        /// Frequency: 3,520.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A7 = 105;
        /// <summary>
        /// MIDI G#7/Ab7 (104)
        /// Piano key number: 84
        /// Frequency: 3,322.44 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs7 = 104;
        /// <summary>
        /// MIDI G#7/Ab7 (104)
        /// Piano key number: 84
        /// Frequency: 3,322.44 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab7 = 104;
        /// <summary>
        /// MIDI G7 (103)
        /// Piano key number: 83
        /// Frequency: 3,135.96 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G7 = 103;
        /// <summary>
        /// MIDI F#7/Gb7 (102)
        /// Piano key number: 82
        /// Frequency: 2,959.96 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs7 = 102;
        /// <summary>
        /// MIDI F#7/Gb7 (102)
        /// Piano key number: 82
        /// Frequency: 2,959.96 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb7 = 102;
        /// <summary>
        /// MIDI F7 (101)
        /// Piano key number: 81
        /// Frequency: 2,793.83 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F7 = 101;
        /// <summary>
        /// MIDI E7 (100)
        /// Piano key number: 80
        /// Frequency: 2,637.02 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E7 = 100;
        /// <summary>
        /// MIDI D#7/Eb7 (99)
        /// Piano key number: 79
        /// Frequency: 2,489.02 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds7 = 99;
        /// <summary>
        /// MIDI D#7/Eb7 (99)
        /// Piano key number: 79
        /// Frequency: 2,489.02 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb7 = 99;
        /// <summary>
        /// MIDI D7 (98)
        /// Piano key number: 78
        /// Frequency: 2,349.32 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D7 = 98;
        /// <summary>
        /// MIDI C#7/Db7 (97)
        /// Piano key number: 77
        /// Frequency: 2,217.46 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs7 = 97;
        /// <summary>
        /// MIDI C#7/Db7 (97)
        /// Piano key number: 77
        /// Frequency: 2,217.46 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db7 = 97;
        /// <summary>
        /// MIDI C7 (96)
        /// Piano key number: 76
        /// Frequency: 2,093.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C7 = 96;
        /// <summary>
        /// MIDI B6 (95)
        /// Piano key number: 75
        /// Frequency: 1,975.53 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B6 = 95;
        /// <summary>
        /// MIDI A#6/Bb6 (94)
        /// Piano key number: 74
        /// Frequency: 1,864.66 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As6 = 94;
        /// <summary>
        /// MIDI A#6/Bb6 (94)
        /// Piano key number: 74
        /// Frequency: 1,864.66 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb6 = 94;
        /// <summary>
        /// MIDI A6 (93)
        /// Piano key number: 73
        /// Frequency: 1,760.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A6 = 93;
        /// <summary>
        /// MIDI G#6/Ab6 (92)
        /// Piano key number: 72
        /// Frequency: 1,661.22 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs6 = 92;
        /// <summary>
        /// MIDI G#6/Ab6 (92)
        /// Piano key number: 72
        /// Frequency: 1,661.22 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab6 = 92;
        /// <summary>
        /// MIDI G6 (91)
        /// Piano key number: 71
        /// Frequency: 1,567.98 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G6 = 91;
        /// <summary>
        /// MIDI F#6/Gb6 (90)
        /// Piano key number: 70
        /// Frequency: 1,479.98 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs6 = 90;
        /// <summary>
        /// MIDI F#6/Gb6 (90)
        /// Piano key number: 70
        /// Frequency: 1,479.98 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb6 = 90;
        /// <summary>
        /// MIDI F6 (89)
        /// Piano key number: 69
        /// Frequency: 1,396.91 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F6 = 89;
        /// <summary>
        /// MIDI E6 (88)
        /// Piano key number: 68
        /// Frequency: 1,318.51 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E6 = 88;
        /// <summary>
        /// MIDI D#6/Eb6 (87)
        /// Piano key number: 67
        /// Frequency: 1,244.51 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds6 = 87;
        /// <summary>
        /// MIDI D#6/Eb6 (87)
        /// Piano key number: 67
        /// Frequency: 1,244.51 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb6 = 87;
        /// <summary>
        /// MIDI D6 (86)
        /// Piano key number: 66
        /// Frequency: 1,174.66 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D6 = 86;
        /// <summary>
        /// MIDI C#6/Db6 (85)
        /// Piano key number: 65
        /// Frequency: 1,108.73 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs6 = 85;
        /// <summary>
        /// MIDI C#6/Db6 (85)
        /// Piano key number: 65
        /// Frequency: 1,108.73 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db6 = 85;
        /// <summary>
        /// MIDI C6 (84)
        /// Piano key number: 64
        /// Frequency: 1,046.50 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C6 = 84;
        /// <summary>
        /// MIDI B5 (83)
        /// Piano key number: 63
        /// Frequency: 987.77 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B5 = 83;
        /// <summary>
        /// MIDI A#5/Bb5 (82)
        /// Piano key number: 62
        /// Frequency: 932.33 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As5 = 82;
        /// <summary>
        /// MIDI A#5/Bb5 (82)
        /// Piano key number: 62
        /// Frequency: 932.33 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb5 = 82;
        /// <summary>
        /// MIDI A5 (81)
        /// Piano key number: 61
        /// Frequency: 880.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A5 = 81;
        /// <summary>
        /// MIDI G#5/Ab5 (80)
        /// Piano key number: 60
        /// Frequency: 830.61 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs5 = 80;
        /// <summary>
        /// MIDI G#5/Ab5 (80)
        /// Piano key number: 60
        /// Frequency: 830.61 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab5 = 80;
        /// <summary>
        /// MIDI G5 (79)
        /// Piano key number: 59
        /// Frequency: 783.99 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G5 = 79;
        /// <summary>
        /// MIDI F#5/Gb5 (78)
        /// Piano key number: 58
        /// Frequency: 739.99 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs5 = 78;
        /// <summary>
        /// MIDI F#5/Gb5 (78)
        /// Piano key number: 58
        /// Frequency: 739.99 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb5 = 78;
        /// <summary>
        /// MIDI F5 (77)
        /// Piano key number: 57
        /// Frequency: 698.46 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F5 = 77;
        /// <summary>
        /// MIDI E5 (76)
        /// Piano key number: 56
        /// Frequency: 659.26 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E5 = 76;
        /// <summary>
        /// MIDI D#5/Eb5 (75)
        /// Piano key number: 55
        /// Frequency: 622.25 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds5 = 75;
        /// <summary>
        /// MIDI D#5/Eb5 (75)
        /// Piano key number: 55
        /// Frequency: 622.25 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb5 = 75;
        /// <summary>
        /// MIDI D5 (74)
        /// Piano key number: 54
        /// Frequency: 587.33 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D5 = 74;
        /// <summary>
        /// MIDI C#5/Db5 (73)
        /// Piano key number: 53
        /// Frequency: 554.37 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs5 = 73;
        /// <summary>
        /// MIDI C#5/Db5 (73)
        /// Piano key number: 53
        /// Frequency: 554.37 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db5 = 73;
        /// <summary>
        /// MIDI C5 (72)
        /// Piano key number: 52
        /// Frequency: 523.25 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C5 = 72;
        /// <summary>
        /// MIDI B4 (71)
        /// Piano key number: 51
        /// Frequency: 493.88 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B4 = 71;
        /// <summary>
        /// MIDI A#4/Bb4 (70)
        /// Piano key number: 50
        /// Frequency: 466.16 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As4 = 70;
        /// <summary>
        /// MIDI A#4/Bb4 (70)
        /// Piano key number: 50
        /// Frequency: 466.16 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb4 = 70;
        /// <summary>
        /// MIDI A4/ConcertPitch (69)
        /// Piano key number: 49
        /// Frequency: 440.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A4 = 69;
        /// <summary>
        /// MIDI A4/ConcertPitch (69)
        /// Piano key number: 49
        /// Frequency: 440.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int ConcertPitch = 69;
        /// <summary>
        /// MIDI G#4/Ab4 (68)
        /// Piano key number: 48
        /// Frequency: 415.30 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs4 = 68;
        /// <summary>
        /// MIDI G#4/Ab4 (68)
        /// Piano key number: 48
        /// Frequency: 415.30 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab4 = 68;
        /// <summary>
        /// MIDI G4 (67)
        /// Piano key number: 47
        /// Frequency: 392.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G4 = 67;
        /// <summary>
        /// MIDI F#4/Gb4 (66)
        /// Piano key number: 46
        /// Frequency: 369.99 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs4 = 66;
        /// <summary>
        /// MIDI F#4/Gb4 (66)
        /// Piano key number: 46
        /// Frequency: 369.99 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb4 = 66;
        /// <summary>
        /// MIDI F4 (65)
        /// Piano key number: 45
        /// Frequency: 349.23 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F4 = 65;
        /// <summary>
        /// MIDI E4 (64)
        /// Piano key number: 44
        /// Frequency: 329.63 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E4 = 64;
        /// <summary>
        /// MIDI D#4/Eb4 (63)
        /// Piano key number: 43
        /// Frequency: 311.13 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds4 = 63;
        /// <summary>
        /// MIDI D#4/Eb4 (63)
        /// Piano key number: 43
        /// Frequency: 311.13 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb4 = 63;
        /// <summary>
        /// MIDI D4 (62)
        /// Piano key number: 42
        /// Frequency: 293.66 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D4 = 62;
        /// <summary>
        /// MIDI C#4/Db4 (61)
        /// Piano key number: 41
        /// Frequency: 277.18 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs4 = 61;
        /// <summary>
        /// MIDI C#4/Db4 (61)
        /// Piano key number: 41
        /// Frequency: 277.18 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db4 = 61;
        /// <summary>
        /// MIDI C4/MiddleC (60)
        /// Piano key number: 40
        /// Frequency: 261.63 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C4 = 60;
        /// <summary>
        /// MIDI C4/MiddleC (60)
        /// Piano key number: 40
        /// Frequency: 261.63 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int MiddleC = 60;
        /// <summary>
        /// MIDI B3 (59)
        /// Piano key number: 39
        /// Frequency: 246.94 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B3 = 59;
        /// <summary>
        /// MIDI A#3/Bb3 (58)
        /// Piano key number: 38
        /// Frequency: 233.08 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As3 = 58;
        /// <summary>
        /// MIDI A#3/Bb3 (58)
        /// Piano key number: 38
        /// Frequency: 233.08 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb3 = 58;
        /// <summary>
        /// MIDI A3 (57)
        /// Piano key number: 37
        /// Frequency: 220.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A3 = 57;
        /// <summary>
        /// MIDI G#3/Ab3 (56)
        /// Piano key number: 36
        /// Frequency: 207.65 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs3 = 56;
        /// <summary>
        /// MIDI G#3/Ab3 (56)
        /// Piano key number: 36
        /// Frequency: 207.65 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab3 = 56;
        /// <summary>
        /// MIDI G3 (55)
        /// Piano key number: 35
        /// Frequency: 196.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G3 = 55;
        /// <summary>
        /// MIDI F#3/Gb3 (54)
        /// Piano key number: 34
        /// Frequency: 185.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs3 = 54;
        /// <summary>
        /// MIDI F#3/Gb3 (54)
        /// Piano key number: 34
        /// Frequency: 185.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb3 = 54;
        /// <summary>
        /// MIDI F3 (53)
        /// Piano key number: 33
        /// Frequency: 174.61 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F3 = 53;
        /// <summary>
        /// MIDI E3 (52)
        /// Piano key number: 32
        /// Frequency: 164.81 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E3 = 52;
        /// <summary>
        /// MIDI D#3/Eb3 (51)
        /// Piano key number: 31
        /// Frequency: 155.56 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds3 = 51;
        /// <summary>
        /// MIDI D#3/Eb3 (51)
        /// Piano key number: 31
        /// Frequency: 155.56 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb3 = 51;
        /// <summary>
        /// MIDI D3 (50)
        /// Piano key number: 30
        /// Frequency: 146.83 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D3 = 50;
        /// <summary>
        /// MIDI C#3/Db3 (49)
        /// Piano key number: 29
        /// Frequency: 138.59 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs3 = 49;
        /// <summary>
        /// MIDI C#3/Db3 (49)
        /// Piano key number: 29
        /// Frequency: 138.59 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db3 = 49;
        /// <summary>
        /// MIDI C3 (48)
        /// Piano key number: 28
        /// Frequency: 130.81 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C3 = 48;
        /// <summary>
        /// MIDI B2 (47)
        /// Piano key number: 27
        /// Frequency: 123.47 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B2 = 47;
        /// <summary>
        /// MIDI A#2/Bb2 (46)
        /// Piano key number: 26
        /// Frequency: 116.54 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As2 = 46;
        /// <summary>
        /// MIDI A#2/Bb2 (46)
        /// Piano key number: 26
        /// Frequency: 116.54 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb2 = 46;
        /// <summary>
        /// MIDI A2 (45)
        /// Piano key number: 25
        /// Frequency: 110.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A2 = 45;
        /// <summary>
        /// MIDI G#2/Ab2 (44)
        /// Piano key number: 24
        /// Frequency: 103.83 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs2 = 44;
        /// <summary>
        /// MIDI G#2/Ab2 (44)
        /// Piano key number: 24
        /// Frequency: 103.83 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab2 = 44;
        /// <summary>
        /// MIDI G2 (43)
        /// Piano key number: 23
        /// Frequency: 98.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G2 = 43;
        /// <summary>
        /// MIDI F#2/Gb2 (42)
        /// Piano key number: 22
        /// Frequency: 92.50 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs2 = 42;
        /// <summary>
        /// MIDI F#2/Gb2 (42)
        /// Piano key number: 22
        /// Frequency: 92.50 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb2 = 42;
        /// <summary>
        /// MIDI F2 (41)
        /// Piano key number: 21
        /// Frequency: 87.31 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F2 = 41;
        /// <summary>
        /// MIDI E2 (40)
        /// Piano key number: 20
        /// Frequency: 82.41 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E2 = 40;
        /// <summary>
        /// MIDI D#2/Eb2 (39)
        /// Piano key number: 19
        /// Frequency: 77.78 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds2 = 39;
        /// <summary>
        /// MIDI D#2/Eb2 (39)
        /// Piano key number: 19
        /// Frequency: 77.78 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb2 = 39;
        /// <summary>
        /// MIDI D2 (38)
        /// Piano key number: 18
        /// Frequency: 73.42 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D2 = 38;
        /// <summary>
        /// MIDI C#2/Db2 (37)
        /// Piano key number: 17
        /// Frequency: 69.30 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs2 = 37;
        /// <summary>
        /// MIDI C#2/Db2 (37)
        /// Piano key number: 17
        /// Frequency: 69.30 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db2 = 37;
        /// <summary>
        /// MIDI C2 (36)
        /// Piano key number: 16
        /// Frequency: 65.41 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C2 = 36;
        /// <summary>
        /// MIDI B1 (35)
        /// Piano key number: 15
        /// Frequency: 61.74 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B1 = 35;
        /// <summary>
        /// MIDI A#1/Bb1 (34)
        /// Piano key number: 14
        /// Frequency: 58.27 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As1 = 34;
        /// <summary>
        /// MIDI A#1/Bb1 (34)
        /// Piano key number: 14
        /// Frequency: 58.27 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb1 = 34;
        /// <summary>
        /// MIDI A1 (33)
        /// Piano key number: 13
        /// Frequency: 55.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A1 = 33;
        /// <summary>
        /// MIDI G#1/Ab1 (32)
        /// Piano key number: 12
        /// Frequency: 51.91 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gs1 = 32;
        /// <summary>
        /// MIDI G#1/Ab1 (32)
        /// Piano key number: 12
        /// Frequency: 51.91 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ab1 = 32;
        /// <summary>
        /// MIDI G1 (31)
        /// Piano key number: 11
        /// Frequency: 49.00 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int G1 = 31;
        /// <summary>
        /// MIDI F#1/Gb1 (30)
        /// Piano key number: 10
        /// Frequency: 46.25 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Fs1 = 30;
        /// <summary>
        /// MIDI F#1/Gb1 (30)
        /// Piano key number: 10
        /// Frequency: 46.25 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Gb1 = 30;
        /// <summary>
        /// MIDI F1 (29)
        /// Piano key number: 9
        /// Frequency: 43.65 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int F1 = 29;
        /// <summary>
        /// MIDI E1 (28)
        /// Piano key number: 8
        /// Frequency: 41.20 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int E1 = 28;
        /// <summary>
        /// MIDI D#1/Eb1 (27)
        /// Piano key number: 7
        /// Frequency: 38.89 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Ds1 = 27;
        /// <summary>
        /// MIDI D#1/Eb1 (27)
        /// Piano key number: 7
        /// Frequency: 38.89 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Eb1 = 27;
        /// <summary>
        /// MIDI D1 (26)
        /// Piano key number: 6
        /// Frequency: 36.71 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int D1 = 26;
        /// <summary>
        /// MIDI C#1/Db1 (25)
        /// Piano key number: 5
        /// Frequency: 34.65 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Cs1 = 25;
        /// <summary>
        /// MIDI C#1/Db1 (25)
        /// Piano key number: 5
        /// Frequency: 34.65 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Db1 = 25;
        /// <summary>
        /// MIDI C1 (24)
        /// Piano key number: 4
        /// Frequency: 32.70 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int C1 = 24;
        /// <summary>
        /// MIDI B0 (23)
        /// Piano key number: 3
        /// Frequency: 30.87 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int B0 = 23;
        /// <summary>
        /// MIDI A#0/Bb0 (22)
        /// Piano key number: 2
        /// Frequency: 29.14 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int As0 = 22;
        /// <summary>
        /// MIDI A#0/Bb0 (22)
        /// Piano key number: 2
        /// Frequency: 29.14 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int Bb0 = 22;
        /// <summary>
        /// MIDI A0 (21)
        /// Piano key number: 1
        /// Frequency: 27.50 ,assuming equal tuning at 440 Hz
        /// </summary>
        public const int A0 = 21;
        #endregion

        #region Commonly Used Constants - MIDI Instruments
        /// <summary>
        /// Represents a non-instrumental track.
        /// This is not a MIDI instrument and is ignored by MIDI engine.
        /// </summary>
        public const int Vocal = -1;

        /// <summary>
        /// Indicates drum channel offset
        /// </summary>
        /// <remarks>
        /// The additional offsets below correspond to drum kit numbers specified in Arachno SoundFont - Version 1.0.sf2 SoundFont
        /// </remarks>
        public const int DrumKitChannelsSpecialOffset = -2;
        /// <summary>
        /// This is a special channel (known as Channel 10) by most MIDI synth for drum kit use
        /// </summary>
        public const int DrumKitInstrumentChannel = 9;
        /// <summary>
        /// Computes the correct instrument number from drum kit channel offset value
        /// </summary>
        public static int GetDrumKitInstrumentNumber(int drumKit)
            => Math.Abs(drumKit - DrumKitChannelsSpecialOffset);
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int StandardDrumKit = DrumKitChannelsSpecialOffset - 0;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int RoomDrumKit = DrumKitChannelsSpecialOffset - 8;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int PowerDrumKit = DrumKitChannelsSpecialOffset - 16;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int ElectricDrumKit = DrumKitChannelsSpecialOffset - 24;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int TR808DrumKit = DrumKitChannelsSpecialOffset - 25;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int JazzDrumKit = DrumKitChannelsSpecialOffset - 32;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int BrushDrumKit = DrumKitChannelsSpecialOffset - 40;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int OrchestralDrumKit = DrumKitChannelsSpecialOffset - 48;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int FixRoomDrumKit = DrumKitChannelsSpecialOffset - 49;
        /// <summary>
        /// Represents a non-instrumental drum kit track.
        /// This is not a MIDI instrument and is special handled by MIDI engine.
        /// </summary>
        public const int MT32DrumKit = DrumKitChannelsSpecialOffset - 127;

        /// <summary>
        /// MIDI Instrument: Acoustic Grand Piano (0)
        /// Class: Piano
        /// </summary>
        public const int AcousticGrandPiano = 0;
        /// <summary>
        /// MIDI Instrument: Bright Acoustic Piano (1)
        /// Class: Piano
        /// </summary>
        public const int BrightAcousticPiano = 1;
        /// <summary>
        /// MIDI Instrument: Electric Grand Piano (2)
        /// Class: Piano
        /// </summary>
        public const int ElectricGrandPiano = 2;
        /// <summary>
        /// MIDI Instrument: Honky-tonk Piano (3)
        /// Class: Piano
        /// </summary>
        public const int HonkytonkPiano = 3;
        /// <summary>
        /// MIDI Instrument: Rhodes Piano (4)
        /// Class: Piano
        /// </summary>
        public const int RhodesPiano = 4;
        /// <summary>
        /// MIDI Instrument: Chorused Piano (5)
        /// Class: Piano
        /// </summary>
        public const int ChorusedPiano = 5;
        /// <summary>
        /// MIDI Instrument: Harpsichord (6)
        /// Class: Piano
        /// </summary>
        public const int Harpsichord = 6;
        /// <summary>
        /// MIDI Instrument: Clavinet (7)
        /// Class: Piano
        /// </summary>
        public const int Clavinet = 7;
        /// <summary>
        /// MIDI Instrument: Celesta (8)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Celesta = 8;
        /// <summary>
        /// MIDI Instrument: Glockenspiel (9)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Glockenspiel = 9;
        /// <summary>
        /// MIDI Instrument: Music Box (10)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int MusicBox = 10;
        /// <summary>
        /// MIDI Instrument: Vibraphone (11)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Vibraphone = 11;
        /// <summary>
        /// MIDI Instrument: Marimba (12)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Marimba = 12;
        /// <summary>
        /// MIDI Instrument: Xylophone (13)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Xylophone = 13;
        /// <summary>
        /// MIDI Instrument: Tubular Bells (14)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int TubularBells = 14;
        /// <summary>
        /// MIDI Instrument: Dulcimer (15)
        /// Class: Chromatic Percussion
        /// </summary>
        public const int Dulcimer = 15;
        /// <summary>
        /// MIDI Instrument: Hammond Organ (16)
        /// Class: Organ
        /// </summary>
        public const int HammondOrgan = 16;
        /// <summary>
        /// MIDI Instrument: Percussive Organ (17)
        /// Class: Organ
        /// </summary>
        public const int PercussiveOrgan = 17;
        /// <summary>
        /// MIDI Instrument: Rock Organ (18)
        /// Class: Organ
        /// </summary>
        public const int RockOrgan = 18;
        /// <summary>
        /// MIDI Instrument: Church Organ (19)
        /// Class: Organ
        /// </summary>
        public const int ChurchOrgan = 19;
        /// <summary>
        /// MIDI Instrument: Reed Organ (20)
        /// Class: Organ
        /// </summary>
        public const int ReedOrgan = 20;
        /// <summary>
        /// MIDI Instrument: Accordion (21)
        /// Class: Organ
        /// </summary>
        public const int Accordion = 21;
        /// <summary>
        /// MIDI Instrument: Harmonica (22)
        /// Class: Organ
        /// </summary>
        public const int Harmonica = 22;
        /// <summary>
        /// MIDI Instrument: Tango Accordion (23)
        /// Class: Organ
        /// </summary>
        public const int TangoAccordion = 23;
        /// <summary>
        /// MIDI Instrument: Acoustic Guitar (nylon) (24)
        /// Class: Guitar
        /// </summary>
        public const int AcousticGuitar1 = 24;
        /// <summary>
        /// MIDI Instrument: Acoustic Guitar (steel) (25)
        /// Class: Guitar
        /// </summary>
        public const int AcousticGuitar2 = 25;
        /// <summary>
        /// MIDI Instrument: Electric Guitar (jazz) (26)
        /// Class: Guitar
        /// </summary>
        public const int ElectricGuitar1 = 26;
        /// <summary>
        /// MIDI Instrument: Electric Guitar (clean) (27)
        /// Class: Guitar
        /// </summary>
        public const int ElectricGuitar2 = 27;
        /// <summary>
        /// MIDI Instrument: Electric Guitar (muted) (28)
        /// Class: Guitar
        /// </summary>
        public const int ElectricGuitar3 = 28;
        /// <summary>
        /// MIDI Instrument: Overdriven Guitar (29)
        /// Class: Guitar
        /// </summary>
        public const int OverdrivenGuitar = 29;
        /// <summary>
        /// MIDI Instrument: Distortion Guitar (30)
        /// Class: Guitar
        /// </summary>
        public const int DistortionGuitar = 30;
        /// <summary>
        /// MIDI Instrument: Guitar Harmonics (31)
        /// Class: Guitar
        /// </summary>
        public const int GuitarHarmonics = 31;
        /// <summary>
        /// MIDI Instrument: Acoustic Bass (32)
        /// Class: Bass
        /// </summary>
        public const int AcousticBass = 32;
        /// <summary>
        /// MIDI Instrument: Electric Bass (finger) (33)
        /// Class: Bass
        /// </summary>
        public const int ElectricBass1 = 33;
        /// <summary>
        /// MIDI Instrument: Electric Bass (pick) (34)
        /// Class: Bass
        /// </summary>
        public const int ElectricBass2 = 34;
        /// <summary>
        /// MIDI Instrument: Fretless Bass (35)
        /// Class: Bass
        /// </summary>
        public const int FretlessBass = 35;
        /// <summary>
        /// MIDI Instrument: Slap Bass 1 (36)
        /// Class: Bass
        /// </summary>
        public const int SlapBass1 = 36;
        /// <summary>
        /// MIDI Instrument: Slap Bass 2 (37)
        /// Class: Bass
        /// </summary>
        public const int SlapBass2 = 37;
        /// <summary>
        /// MIDI Instrument: Synth Bass 1 (38)
        /// Class: Bass
        /// </summary>
        public const int SynthBass1 = 38;
        /// <summary>
        /// MIDI Instrument: Synth Bass 2 (39)
        /// Class: Bass
        /// </summary>
        public const int SynthBass2 = 39;
        /// <summary>
        /// MIDI Instrument: Violin (40)
        /// Class: Strings
        /// </summary>
        public const int Violin = 40;
        /// <summary>
        /// MIDI Instrument: Viola (41)
        /// Class: Strings
        /// </summary>
        public const int Viola = 41;
        /// <summary>
        /// MIDI Instrument: Cello (42)
        /// Class: Strings
        /// </summary>
        public const int Cello = 42;
        /// <summary>
        /// MIDI Instrument: Contrabass (43)
        /// Class: Strings
        /// </summary>
        public const int Contrabass = 43;
        /// <summary>
        /// MIDI Instrument: Tremolo Strings (44)
        /// Class: Strings
        /// </summary>
        public const int TremoloStrings = 44;
        /// <summary>
        /// MIDI Instrument: Pizzicato Strings (45)
        /// Class: Strings
        /// </summary>
        public const int PizzicatoStrings = 45;
        /// <summary>
        /// MIDI Instrument: Orchestral Harp (46)
        /// Class: Strings
        /// </summary>
        public const int OrchestralHarp = 46;
        /// <summary>
        /// MIDI Instrument: Timpani (47)
        /// Class: Strings
        /// </summary>
        public const int Timpani = 47;
        /// <summary>
        /// MIDI Instrument: String Ensemble 1 (48)
        /// Class: Ensemble
        /// </summary>
        public const int StringEnsemble1 = 48;
        /// <summary>
        /// MIDI Instrument: String Ensemble 2 (49)
        /// Class: Ensemble
        /// </summary>
        public const int StringEnsemble2 = 49;
        /// <summary>
        /// MIDI Instrument: Synth Strings 1 (50)
        /// Class: Ensemble
        /// </summary>
        public const int SynthStrings1 = 50;
        /// <summary>
        /// MIDI Instrument: Synth Strings 2 (51)
        /// Class: Ensemble
        /// </summary>
        public const int SynthStrings2 = 51;
        /// <summary>
        /// MIDI Instrument: Choir Aahs (52)
        /// Class: Ensemble
        /// </summary>
        public const int ChoirAahs = 52;
        /// <summary>
        /// MIDI Instrument: Voice Oohs (53)
        /// Class: Ensemble
        /// </summary>
        public const int VoiceOohs = 53;
        /// <summary>
        /// MIDI Instrument: Synth Voice (54)
        /// Class: Ensemble
        /// </summary>
        public const int SynthVoice = 54;
        /// <summary>
        /// MIDI Instrument: Orchestra Hit (55)
        /// Class: Ensemble
        /// </summary>
        public const int OrchestraHit = 55;
        /// <summary>
        /// MIDI Instrument: Trumpet (56)
        /// Class: Brass
        /// </summary>
        public const int Trumpet = 56;
        /// <summary>
        /// MIDI Instrument: Trombone (57)
        /// Class: Brass
        /// </summary>
        public const int Trombone = 57;
        /// <summary>
        /// MIDI Instrument: Tuba (58)
        /// Class: Brass
        /// </summary>
        public const int Tuba = 58;
        /// <summary>
        /// MIDI Instrument: Muted Trumpet (59)
        /// Class: Brass
        /// </summary>
        public const int MutedTrumpet = 59;
        /// <summary>
        /// MIDI Instrument: French Horn (60)
        /// Class: Brass
        /// </summary>
        public const int FrenchHorn = 60;
        /// <summary>
        /// MIDI Instrument: Brass Section (61)
        /// Class: Brass
        /// </summary>
        public const int BrassSection = 61;
        /// <summary>
        /// MIDI Instrument: Synth Brass 1 (62)
        /// Class: Brass
        /// </summary>
        public const int SynthBrass1 = 62;
        /// <summary>
        /// MIDI Instrument: Synth Brass 2 (63)
        /// Class: Brass
        /// </summary>
        public const int SynthBrass2 = 63;
        /// <summary>
        /// MIDI Instrument: Soprano Sax (64)
        /// Class: Reed
        /// </summary>
        public const int SopranoSax = 64;
        /// <summary>
        /// MIDI Instrument: Alto Sax (65)
        /// Class: Reed
        /// </summary>
        public const int AltoSax = 65;
        /// <summary>
        /// MIDI Instrument: Tenor Sax (66)
        /// Class: Reed
        /// </summary>
        public const int TenorSax = 66;
        /// <summary>
        /// MIDI Instrument: Baritone Sax (67)
        /// Class: Reed
        /// </summary>
        public const int BaritoneSax = 67;
        /// <summary>
        /// MIDI Instrument: Oboe (68)
        /// Class: Reed
        /// </summary>
        public const int Oboe = 68;
        /// <summary>
        /// MIDI Instrument: English Horn (69)
        /// Class: Reed
        /// </summary>
        public const int EnglishHorn = 69;
        /// <summary>
        /// MIDI Instrument: Bassoon (70)
        /// Class: Reed
        /// </summary>
        public const int Bassoon = 70;
        /// <summary>
        /// MIDI Instrument: Clarinet (71)
        /// Class: Reed
        /// </summary>
        public const int Clarinet = 71;
        /// <summary>
        /// MIDI Instrument: Piccolo (72)
        /// Class: Pipe
        /// </summary>
        public const int Piccolo = 72;
        /// <summary>
        /// MIDI Instrument: Flute (73)
        /// Class: Pipe
        /// </summary>
        public const int Flute = 73;
        /// <summary>
        /// MIDI Instrument: Recorder (74)
        /// Class: Pipe
        /// </summary>
        public const int Recorder = 74;
        /// <summary>
        /// MIDI Instrument: Pan Flute (75)
        /// Class: Pipe
        /// </summary>
        public const int PanFlute = 75;
        /// <summary>
        /// MIDI Instrument: Bottle Blow (76)
        /// Class: Pipe
        /// </summary>
        public const int BottleBlow = 76;
        /// <summary>
        /// MIDI Instrument: Shakuhachi (77)
        /// Class: Pipe
        /// </summary>
        public const int Shakuhachi = 77;
        /// <summary>
        /// MIDI Instrument: Whistle (78)
        /// Class: Pipe
        /// </summary>
        public const int Whistle = 78;
        /// <summary>
        /// MIDI Instrument: Ocarina (79)
        /// Class: Pipe
        /// </summary>
        public const int Ocarina = 79;
        /// <summary>
        /// MIDI Instrument: Lead 1 (square) (80)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead1 = 80;
        /// <summary>
        /// MIDI Instrument: Lead 2 (sawtooth) (81)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead2 = 81;
        /// <summary>
        /// MIDI Instrument: Lead 3 (calliope lead) (82)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead3 = 82;
        /// <summary>
        /// MIDI Instrument: Lead 4 (chiffer lead) (83)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead4 = 83;
        /// <summary>
        /// MIDI Instrument: Lead 5 (charang) (84)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead5 = 84;
        /// <summary>
        /// MIDI Instrument: Lead 6 (voice) (85)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead6 = 85;
        /// <summary>
        /// MIDI Instrument: Lead 7 (fifths) (86)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead7 = 86;
        /// <summary>
        /// MIDI Instrument: Lead 8 (brass + lead) (87)
        /// Class: Synth Lead
        /// </summary>
        public const int Lead8 = 87;
        /// <summary>
        /// MIDI Instrument: Pad 1 (new age) (88)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad1 = 88;
        /// <summary>
        /// MIDI Instrument: Pad 2 (warm) (89)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad2 = 89;
        /// <summary>
        /// MIDI Instrument: Pad 3 (polysynth) (90)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad3 = 90;
        /// <summary>
        /// MIDI Instrument: Pad 4 (choir) (91)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad4 = 91;
        /// <summary>
        /// MIDI Instrument: Pad 5 (bowed) (92)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad5 = 92;
        /// <summary>
        /// MIDI Instrument: Pad 6 (metallic) (93)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad6 = 93;
        /// <summary>
        /// MIDI Instrument: Pad 7 (halo) (94)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad7 = 94;
        /// <summary>
        /// MIDI Instrument: Pad 8 (sweep) (95)
        /// Class: Synth Pad
        /// </summary>
        public const int Pad8 = 95;
        /// <summary>
        /// MIDI Instrument: FX 1 (rain) (96)
        /// Class: Synth Effects
        /// </summary>
        public const int FX1 = 96;
        /// <summary>
        /// MIDI Instrument: FX 2 (soundtrack) (97)
        /// Class: Synth Effects
        /// </summary>
        public const int FX2 = 97;
        /// <summary>
        /// MIDI Instrument: FX 3 (crystal) (98)
        /// Class: Synth Effects
        /// </summary>
        public const int FX3 = 98;
        /// <summary>
        /// MIDI Instrument: FX 4 (atmosphere) (99)
        /// Class: Synth Effects
        /// </summary>
        public const int FX4 = 99;
        /// <summary>
        /// MIDI Instrument: FX 5 (brightness) (100)
        /// Class: Synth Effects
        /// </summary>
        public const int FX5 = 100;
        /// <summary>
        /// MIDI Instrument: FX 6 (goblins) (101)
        /// Class: Synth Effects
        /// </summary>
        public const int FX6 = 101;
        /// <summary>
        /// MIDI Instrument: FX 7 (echoes) (102)
        /// Class: Synth Effects
        /// </summary>
        public const int FX7 = 102;
        /// <summary>
        /// MIDI Instrument: FX 8 (sci-fi) (103)
        /// Class: Synth Effects
        /// </summary>
        public const int FX8 = 103;
        /// <summary>
        /// MIDI Instrument: Sitar (104)
        /// Class: Ethnic
        /// </summary>
        public const int Sitar = 104;
        /// <summary>
        /// MIDI Instrument: Banjo (105)
        /// Class: Ethnic
        /// </summary>
        public const int Banjo = 105;
        /// <summary>
        /// MIDI Instrument: Shamisen (106)
        /// Class: Ethnic
        /// </summary>
        public const int Shamisen = 106;
        /// <summary>
        /// MIDI Instrument: Koto (107)
        /// Class: Ethnic
        /// </summary>
        public const int Koto = 107;
        /// <summary>
        /// MIDI Instrument: Kalimba (108)
        /// Class: Ethnic
        /// </summary>
        public const int Kalimba = 108;
        /// <summary>
        /// MIDI Instrument: Bagpipe (109)
        /// Class: Ethnic
        /// </summary>
        public const int Bagpipe = 109;
        /// <summary>
        /// MIDI Instrument: Fiddle (110)
        /// Class: Ethnic
        /// </summary>
        public const int Fiddle = 110;
        /// <summary>
        /// MIDI Instrument: Shana (111)
        /// Class: Ethnic
        /// </summary>
        public const int Shana = 111;
        /// <summary>
        /// MIDI Instrument: Tinkle Bell (112)
        /// Class: Percussive
        /// </summary>
        public const int TinkleBell = 112;
        /// <summary>
        /// MIDI Instrument: Agogo (113)
        /// Class: Percussive
        /// </summary>
        public const int Agogo = 113;
        /// <summary>
        /// MIDI Instrument: Steel Drums (114)
        /// Class: Percussive
        /// </summary>
        public const int SteelDrums = 114;
        /// <summary>
        /// MIDI Instrument: Woodblock (115)
        /// Class: Percussive
        /// </summary>
        public const int Woodblock = 115;
        /// <summary>
        /// MIDI Instrument: Taiko Drum (116)
        /// Class: Percussive
        /// </summary>
        public const int TaikoDrum = 116;
        /// <summary>
        /// MIDI Instrument: Melodic Tom (117)
        /// Class: Percussive
        /// </summary>
        public const int MelodicTom = 117;
        /// <summary>
        /// MIDI Instrument: Synth Drum (118)
        /// Class: Percussive
        /// </summary>
        public const int SynthDrum = 118;
        /// <summary>
        /// MIDI Instrument: Reverse Cymbal (119)
        /// Class: Percussive
        /// </summary>
        public const int ReverseCymbal = 119;
        /// <summary>
        /// MIDI Instrument: Guitar Fret Noise (120)
        /// Class: Sound Effects
        /// </summary>
        public const int GuitarFretNoise = 120;
        /// <summary>
        /// MIDI Instrument: Breath Noise (121)
        /// Class: Sound Effects
        /// </summary>
        public const int BreathNoise = 121;
        /// <summary>
        /// MIDI Instrument: Seashore (122)
        /// Class: Sound Effects
        /// </summary>
        public const int Seashore = 122;
        /// <summary>
        /// MIDI Instrument: Bird Tweet (123)
        /// Class: Sound Effects
        /// </summary>
        public const int BirdTweet = 123;
        /// <summary>
        /// MIDI Instrument: Telephone Ring (124)
        /// Class: Sound Effects
        /// </summary>
        public const int TelephoneRing = 124;
        /// <summary>
        /// MIDI Instrument: Helicopter (125)
        /// Class: Sound Effects
        /// </summary>
        public const int Helicopter = 125;
        /// <summary>
        /// MIDI Instrument: Applause (126)
        /// Class: Sound Effects
        /// </summary>
        public const int Applause = 126;
        /// <summary>
        /// MIDI Instrument: Gunshot (127)
        /// Class: Sound Effects
        /// </summary>
        public const int Gunshot = 127;

        #endregion

        #region MIDI Helper
        /// <summary>
        /// Converts MIDI note number to base frequency per $f=440*2^((n-69)/12)$ - assuming equal tuning based on A4=a'=440 Hz.
        /// See https://www.inspiredacoustics.com/en/MIDI_note_numbers_and_center_frequencies for more details.
        /// </summary>
        /// <param name="noteNumber">MIDI note number e.g. 60</param>
        /// <returns>Base frequency in HZ</returns>
        public static double ConvertNoeFrequency(int noteNumber)
        {
            return (double) 440 * Math.Pow(2, (noteNumber - 69) / 12);
        }
        #endregion

        #region Quick Play
        /// <summary>
        /// Play duel channel floating-point samples
        /// </summary>
        /// <remarks>
        /// Notice for the purpose of ease of use (and fluent API), we are not releasing any resources.
        /// TODO: It's going to be hard if we want to handle proper resource releasing; It's also not necessasry for the purpose it serves.
        /// </remarks>
        public static WaveOutEvent Play(int sampleRate, float[] left, float[] right)
        {
            byte[] bytes = ConvertChannels(left, right);
            MemoryStream memoryStream = new(bytes);
            RawSourceWaveStream waveStream = new(memoryStream, new WaveFormat(sampleRate, BitsPerSample, 2));
            WaveOutEvent outputDevice = new();

            outputDevice.Init(waveStream);
            outputDevice.Play();
            return outputDevice;
        }
        /// <summary>
        /// Play single-channel floating-point samples
        /// </summary>
        /// <remarks>
        /// Notice for the purpose of ease of use (and fluent API), we are not releasing any resources.
        /// TODO: It's going to be hard if we want to handle proper resource releasing; It's also not necessasry for the purpose it serves.
        /// </remarks>
        public static WaveOutEvent Play(int sampleRate, float[] channel)
        {
            var bytes = ConvertChannel(channel);
            var memoryStream = new MemoryStream(bytes);
            var waveStream = new RawSourceWaveStream(memoryStream, new WaveFormat(sampleRate, BitsPerSample, 1));
            var outputDevice = new WaveOutEvent();

            outputDevice.Init(waveStream);
            outputDevice.Play();
            return outputDevice;
        }
        /// <summary>
        /// Play score object.
        /// </summary>
        /// <remarks>
        /// Notice for the purpose of ease of use (and fluent API), we are not releasing any resources.
        /// TODO: It's going to be hard if we want to handle proper resource releasing; It's also not necessasry for the purpose it serves.
        /// </remarks>
        public WaveOutEvent Play(Score score, out int durationInMilliseconds)
        {
            int sampleRate = DefaultSampleRate;

            new Orchestrator(sampleRate, SoundFontFilePath)
                .Orchestrate(score, out float[] left, out float[] right, out durationInMilliseconds);

            return Play(sampleRate, left, right);
        }

        /// <summary>
        /// Play specific MIDI file
        /// </summary>
        /// <remarks>
        /// Notice for the purpose of ease of use (and fluent API), we are not releasing any resources.
        /// TODO: It's going to be hard if we want to handle proper resource releasing; It's also not necessasry for the purpose it serves.
        /// </remarks>
        public WaveOutEvent PlayMIDIFile(string filePath, out int durationInMilliseconds)
        {
            int sampleRate = DefaultSampleRate;
            Synthesizer synthesizer = new(SoundFontFilePath, sampleRate);
            MidiFile midiFile = new(filePath);

            MidiFileSequencer sequencer = new(synthesizer);
            sequencer.Play(midiFile, false);

            float[] left = new float[(int)(sampleRate * midiFile.Length.TotalSeconds)];
            float[] right = new float[(int)(sampleRate * midiFile.Length.TotalSeconds)];
            sequencer.Render(left, right);

            durationInMilliseconds = (int)(midiFile.Length.TotalSeconds * 1000);
            return Play(sampleRate, left);
        }
        /// <summary>
        /// Parse and play ascore.
        /// </summary>
        public WaveOutEvent Play(string scoreScript, out int durationInMilliseconds)
        {
            Score score = MusicalScoreParser.Parse(scoreScript);
            return Play(score, out durationInMilliseconds);
        }
        #endregion

        #region Helpers
        /// <remarks>
        /// Splits an 16-bit PCM duel channel audio byte stream into two channels of float wave data
        /// </remarks>
        public static (float[] Left, float[] Right) SplitChannels(byte[] pcm)
        {
            int stride = 2 * sizeof(short);
            int sampleCount = pcm.Length / stride;
            float[] left = new float[sampleCount];
            float[] right = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                left[i] = BitConverter.ToInt16(new byte[] { pcm[i * stride], pcm[i * stride + 1] }) / (float)short.MaxValue;
                right[i] = BitConverter.ToInt16(new byte[] { pcm[i * stride + 2], pcm[i * stride + 3] }) / (float)short.MaxValue;
            }

            return (left, right);
        }

        /// <remarks>
        /// Splits a single precision (PCM-like) IEEE float duel channel audio stream into two channels of float wave data
        /// </remarks>
        /// <param name="samples">Values assumed to be within [-1, 1]</param>
        public static (float[] Left, float[] Right) SplitChannels(float[] samples)
        {
            int stride = 2;
            int sampleCount = samples.Length / stride;
            float[] left = new float[sampleCount];
            float[] right = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                left[i] = samples[i * stride];
                right[i] = samples[i * stride + 1];
            }

            return (left, right);
        }

        /// <param name="left">Values must be within [-1, 1]</param>
        /// <param name="right">Values must be within [-1, 1]</param>
        /// <remarks>
        /// PCM audio supports up to 8 channels of audio, which includes left, right, center, left surround, right surround, left back, right back, and a subwoofer channel.
        /// </remarks>
        public static byte[] ConvertChannels(float[] left, float[] right)
        {
            var bytes = left
                .Zip(right, (left, right) => (Left: left, Right: right))
                .SelectMany(channels => {
                    // This automatically handles both negative and positive spectrum because channels value could be negative
                    var sampleL = (short)(channels.Left * short.MaxValue);
                    var sampleR = (short)(channels.Right * short.MaxValue);

                    var bytesL = BitConverter.GetBytes(sampleL);
                    var bytesR = BitConverter.GetBytes(sampleR);
                    // Each sample is just byte sequence of a short
                    // Samples for each channel follows each other
                    return new byte[] {
                        bytesL[0], bytesL[1],
                        bytesR[0], bytesR[1]
                    };
                })
                .ToArray();
            return bytes;
        }
        /// <summary>
        /// Converts a single channel float (range in [-1, 1]) waveform into PCM (16-bit short)
        /// </summary>
        public static byte[] ConvertChannel(float[] channel)
        {
            var bytes = channel
                .SelectMany(sample => {
                    // This automatically handles both negative and positive spectrum because channels value could be negative
                    var value = (short)(sample * short.MaxValue);
                    var bytes = BitConverter.GetBytes(value);
                    // Each sample is just byte sequence of a short
                    return new byte[] {
                        bytes[0], bytes[1]
                    };
                })
                .ToArray();
            return bytes;
        }
        #endregion
    }
}