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

## Main Reference

* NAudio: https://github.com/naudio
* Melty Synth: https://github.com/sinshu/meltysynth
* NAudio synth: https://github.com/naudio/NAudio/blob/master/Docs/PlaySineWave.md
* NAudio raw sample stream: https://github.com/naudio/NAudio/blob/master/Docs/RawSourceWaveStream.md
* MIDI playback: https://github.com/sinshu/meltysynth/tree/main/Examples/NAudio
* MIDI Note Numbres and Center Frequencies: https://www.inspiredacoustics.com/en/MIDI_note_numbers_and_center_frequencies
* MIDI Instrument number: https://fmslogo.sourceforge.io/manual/midi-instrument.html
* Guido Music Notation: https://wiki.ccarh.org/wiki/Guido_Music_Notation