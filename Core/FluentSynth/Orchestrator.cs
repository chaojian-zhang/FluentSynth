using MeltySynth;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Reflection.PortableExecutable;

namespace FluentSynth
{
    /// <summary>
    /// Orchestrate a score into waveform
    /// </summary>
    public sealed class Orchestrator
    {
        #region Properties
        /// <summary>
        /// Output waveform sample rate
        /// </summary>
        public int SampleRate { get; }
        /// <summary>
        /// MIDI synth source sound font
        /// </summary>
        public string SoundFontFilePath { get; }
        #endregion

        #region Construction
        /// <summary>
        /// Initialize orchestrator with essential parameters
        /// </summary>
        public Orchestrator(int sampleRate, string soundFontFilePath)
        {
            SampleRate = sampleRate;
            SoundFontFilePath = soundFontFilePath;
        }
        #endregion

        #region Method
        /// <summary>
        /// Generate waveform
        /// </summary>
        public void Orchestrate(Score score, out float[] left, out float[] right, out int durationInMilliseconds)
        {
            // The output buffer
            int totalSamples = score.GetTotalSamples(SampleRate);
            float[] leftBuffer = new float[totalSamples];
            float[] rightBuffer = new float[totalSamples];

            // MIDI engine
            Synthesizer synthesizer = new(SoundFontFilePath, SampleRate);
            MixMIDI(score, leftBuffer, rightBuffer, synthesizer);

            // Vocal Engine
            MixVocals(score, in leftBuffer, in rightBuffer, SampleRate);

            // Return
            durationInMilliseconds = (int)(score.TotalSeconds * 1000);
            left = leftBuffer;
            right = rightBuffer;
        }
        #endregion

        #region Routines
        /// <summary>
        /// Mix-in samples using MIDI engine
        /// </summary>
        private void MixMIDI(Score score, float[] left, float[] right, Synthesizer synthesizer)
        {
            for (int m = 0; m < score.Measures.Length; m++)
            {
                Measure measure = score.Measures[m];
                int spanStartIndex = (int)(m * score.MeasureSizeInSeconds * SampleRate); // TODO: May have alignment problem

                synthesizer.NoteOffAll(true);

                for (int channel = 0; channel < measure.Sections.Length; channel++)
                {
                    // A channel is an independent path over which messages travel to their destination. There are 16 channels per MIDI device. A track in your sequencer program plays one instrument over a single channel. The MIDI messages in the track find their way to the instrument over that channel.
                    MeasureSection section = measure.Sections[channel];

                    // Don't handle vocals in MIDI engine
                    if (section.MIDIInstrument == Synth.Vocal)
                        continue;

                    // Update instrument
                    synthesizer.ProcessMidiMessage(channel, 0xC0, section.MIDIInstrument, 0); // Send a program change command (0xC0) to the synthesizer in order to change instrument

                    int previousNoteLengths = 0;
                    for (int b = 0; b < section.Notes.Length; b++)
                    {
                        Note note = section.Notes[b];
                        foreach (NotePitch n in note.Pitches)
                        {
                            if (n.IsVocal)
                                throw new ApplicationException("Vocals are not handled in MIDI engine.");
                            else if (n.Pitch > 0)
                                synthesizer.NoteOn(channel, n.Pitch, note.Velocity); // Signature: Channel (0 for both), key, velocity
                            else
                                synthesizer.NoteOffAll(channel, false); // With release
                        }

                        // TODO: Change to block-based rendering per example here - https://github.com/sinshu/meltysynth
                        // We are almost there with our current multiple-section (channel) per measure structure
                        int noteSize = score.GetBeatSizeInFloats(SampleRate) * (int)note.GetBeatCount(score.BeatSize);
                        Span<float> leftSpan = new(left, spanStartIndex + previousNoteLengths, noteSize);
                        Span<float> rightSpan = new(right, spanStartIndex + previousNoteLengths, noteSize);
                        synthesizer.Render(leftSpan, rightSpan);

                        previousNoteLengths += noteSize;
                    }
                }
            }
        }
        private void MixVocals(Score score, in float[] left, in float[] right, int sampleRate)
        {
            for (int m = 0; m < score.Measures.Length; m++)
            {
                Measure measure = score.Measures[m];
                int spanStartIndex = (int)(m * score.MeasureSizeInSeconds * SampleRate);

                for (int channel = 0; channel < measure.Sections.Length; channel++)
                {
                    MeasureSection section = measure.Sections[channel];

                    // Don't handle MIDI instruments
                    if (section.MIDIInstrument != Synth.Vocal)
                        continue;

                    int previousNoteLengths = 0;
                    for (int b = 0; b < section.Notes.Length; b++)
                    {
                        Note note = section.Notes[b];
                        foreach (NotePitch n in note.Pitches)
                        {
                            if (!n.IsVocal)
                                continue; // Skip pauses
                            else
                            {
                                AudioFileReader reader = new AudioFileReader(score.Vocals[n.VocalName]);
                                int audioLength = (int)reader.Length;
                                int clipLength = Math.Min(audioLength, score.GetTotalSamples(SampleRate) - spanStartIndex - previousNoteLengths);
                                Span<float> leftSpan = new(left, spanStartIndex + previousNoteLengths, clipLength);
                                Span<float> rightSpan = new(right, spanStartIndex + previousNoteLengths, clipLength);

                                MixIn(leftSpan, rightSpan, sampleRate, reader);
                            }
                        }

                        int noteSize = score.GetBeatSizeInFloats(sampleRate) * (int)note.GetBeatCount(score.BeatSize);
                        previousNoteLengths += noteSize;
                    }
                }
            }
        }
        private static void MixIn(Span<float> left, Span<float> right, int sampleRate, AudioFileReader mixin)
        {
            (float[] Left, float[] Right) result = MixAudio(left.ToArray(), right.ToArray(), sampleRate, mixin);

            result.Left.AsSpan().CopyTo(left);
            result.Right.AsSpan().CopyTo(right);
        }
        private static (float[] Left, float[] Right) MixAudio(float[] left, float[] right, int sampleRate, AudioFileReader reader)
        {
            WaveFormat waveFormat = new(sampleRate, Synth.BitsPerSample, 2);
            byte[] bytes = Synth.ConvertChannels(left, right);

            MemoryStream memoryStream = new(bytes);
            RawSourceWaveStream waveStream = new(memoryStream, waveFormat);
            
            int sampleCount = new int[] { left.Length, right.Length, (int)reader.Length }.Min();
            float[] buffer = new float[sampleCount];

            MixingSampleProvider mixer = new(new[] { waveStream.ToSampleProvider(), ConvertWaveFormat(waveFormat, reader).ToSampleProvider() });
            int read = mixer.Read(buffer, 0, sampleCount);

            return Synth.SplitChannels(buffer);
        }
        #endregion

        #region Helpers
        private static IWaveProvider ConvertWaveFormat(WaveFormat targetWaveFormat, AudioFileReader audioReader)
        {
            if (audioReader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                WaveFloatTo16Provider pcmConverter = new(audioReader);
                return new WaveFormatConversionProvider(targetWaveFormat, pcmConverter);
            }
            else if (audioReader.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                return new WaveFormatConversionStream(targetWaveFormat, audioReader);
            throw new ApplicationException("Unexpected format.");
        }
        #endregion
    }
}
