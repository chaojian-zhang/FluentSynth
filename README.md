# Fluent Synth

This project provides a C# Fluent API music composition library for use in a scripting environment e.g. [Pure](https://github.com/pure-the-Language/Pure/). Many years ago when Haskell was still new they had [Euterpea](https://www.euterpea.com/) which is a fun concept but tedious to use because Haskell is hard to use, and music composition is inherently a procedural thing so a functional approach is not the best way to model it.

Main inspirations:

0. Arguably the main inspiration is [MultiFractalTerrain API](https://github.com/Charles-Zhang-Project-Nine-Experiments/MultiFractalTerrain) which has the potential to provide full suite of World Machine capabilities in a programmable and open-source fashion and is a proof-of-concept of Pure-based Fluent scripting fluency. 
1. NAudio for audio playback and raw waveform assembly.
2. [MeltySynth](https://github.com/sinshu/meltysynth) for MIDI assembly.

The [Core library](./Core/FluentSynth) will provide preliminary FluentSynth for Pure-oriented MIDI-supported but more targeting raw waveform procedural generation

## Methodology (Preliminary Note)

This library provides raw API for manipulating and assembling sin waves and notes, and relies on NAudio for music playback. By reading or creating MIDI files on the fly, one can represent musical ideas on a higher level.

The library (FluentSynth) provide two sets of APIs: 1) For raw fixed-size waveform manipulation; 2) For MIDI-based synthesization aka. data-driven approach.

Being able to construct raw waveforms from scratch is useful for basic musical note and chord exploration, but for more efficient music composition, using scores and MIDI is more convinient.
The raw waveform based API also has the advantage that when used in a scripting environment it can be entirely procedural (though the same can be argued for MIDI based approach).

For musical scores, we use a partial and modified version of [Guido Music Notation](https://en.wikipedia.org/wiki/GUIDO_music_notation).

## Fluent Synth Music Notation (FSMN Score)

An entire composition is broken into **measures**, and each measure consists of **beats**. The default time signature is `4/4`.

For specifically, either single sequence of notes, or sequence of measures, or entire composition can be specified as input:

* Sequence of notes, separated by space: `c d e f g h a b`
* Sequence of measures: `[c d e f]`
* Entire composition (in a single line): `(120) 4/4 {Piano} [c c g g] [a a g/2]`

### Musical Notes

Musical notes can be represented using the typical diatonic musical notes as `A B C D E F G` or numerically as `1 2 3 4 5 6 7`, or as solfège: `do re mi fa sol la ti/si`.  
Note names are case-insensitive. Numerical and solfège specification are in the key of C. Note names never contain spaces.

When numerically or as solfège, one can use `'` as suffix to raise an octave and use `_` as suffix to lower an octave, e.g. `1'` is two octave higher than `1_`.  
When using English letters, one can use accidentals and numbers to denote which octave and tone the note is, e.g. `A#6` (MIDI 94) is piano key 74, and `C6` (MIDI 84) is piano key 64, also known as Soprano C (High C).

A complete note consists of three components: note name, velocity (also known as attack), and duration. The complete form looks like this: `<Note Name>/<Duration>@<Attack>`. E.g. `[c/1 c d/2 d e/4 e f/8 f g/16 g a/32 a]`.   
Notice durations are not sticky, and by default the note duration is the length specified in the denominator in the time signature.

### Duration

Default duration for a note is a quarter note. To indicate a different rhythm, indicate it after any octave indicator by writing a slash `/` followed by the duration of the note in terms of divisions per whole note: `[c/1 c d/2 d e/4 e f/8 f g/16 g a/32 a]`.

|rhythm|code|
|whole note|1|
|half note|2|
|quarter note|4|
|eighth note|8|
|sixteenth note|16|
|thiry-second note|32|

### Chords

A note can be played together with other notes at the same time by using `|` to connect them. E.g. (Middle C, E, G with different attacks) `C|E|G/4@100`; When specified in this fashion, the notes must have the same duration and velocity.

### Accidentals

Sharps are indicated by a pound sign (#) and flats are indicated by an ampersand (&): `[c# d& e f# g&]`.

At the moment there is no capacity to parse something like `f##` or `g&&` - you need to manually convert it to `g` or `f`.

## TODO

The overall infrastructure is established, in the future, the likely site of improvement is either [Orchestrator](./Core/FluentSynth/Orchestrator.cs) or [MusicalScoreParser](./Core/FluentSynth/MusicalScoreParser.cs).

- [ ] Convert of FSMN to .MIDI file.
- [ ] Convert of FSMN/MIDI file into mp3/wav file.

## Main Reference

* NAudio: https://github.com/naudio
* Melty Synth: https://github.com/sinshu/meltysynth
* NAudio synth: https://github.com/naudio/NAudio/blob/master/Docs/PlaySineWave.md
* NAudio raw sample stream: https://github.com/naudio/NAudio/blob/master/Docs/RawSourceWaveStream.md
* MIDI playback: https://github.com/sinshu/meltysynth/tree/main/Examples/NAudio
* MIDI Note Numbres and Center Frequencies: https://www.inspiredacoustics.com/en/MIDI_note_numbers_and_center_frequencies
* MIDI Instrument number: https://fmslogo.sourceforge.io/manual/midi-instrument.html
* Guido Music Notation: https://wiki.ccarh.org/wiki/Guido_Music_Notation