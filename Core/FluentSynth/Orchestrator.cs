using MeltySynth;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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
            double measureSizeInSeconds = (double)score.BeatsPerMeasure / score.BPM * 60;
            double totalSeconds = (int)Math.Ceiling(score.Measures.Length * measureSizeInSeconds);

            // The output buffer
            int totalSamples = (int)(totalSeconds * SampleRate);  // TODO: May have alignment problem
            float[] leftBuffer = new float[totalSamples];
            float[] rightBuffer = new float[totalSamples];

            // MIDI engine
            Synthesizer synthesizer = new(SoundFontFilePath, SampleRate);
            MixMIDI(score, leftBuffer, rightBuffer, synthesizer);

            // Vocal Engine
            (float[] Left, float[] Right) result = MixVocals(score, in leftBuffer, in rightBuffer, SampleRate);

            // Return
            durationInMilliseconds = (int)(totalSeconds * 1000);
            left = result.Left;
            right = result.Right;
        }
        #endregion

        #region Routines
        /// <summary>
        /// Mix-in samples using MIDI engine
        /// </summary>
        private void MixMIDI(Score score, float[] left, float[] right, Synthesizer synthesizer)
        {
            double measureSizeInSeconds = (double)score.BeatsPerMeasure / score.BPM * 60;
            int measureSizeInFloats = (int)(measureSizeInSeconds * SampleRate);  // TODO: May have alignment problem
            int beatSizeInFloats = measureSizeInFloats / score.BeatsPerMeasure;

            for (int m = 0; m < score.Measures.Length; m++)
            {
                Measure measure = score.Measures[m];
                int spanStartIndex = (int)(m * measureSizeInSeconds * SampleRate); // TODO: May have alignment problem

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
                                throw new NotImplementedException(); // synthesizer.NoteOffAll(false); // With release
                        }

                        // TODO: Change to block-based rendering per example here - https://github.com/sinshu/meltysynth
                        // We are almost there with our current multiple-section (channel) per measure structure
                        int noteSize = beatSizeInFloats * (int)note.GetBeatCount(score.BeatSize);
                        Span<float> leftSpan = new(left, spanStartIndex + previousNoteLengths, noteSize);
                        Span<float> rightSpan = new(right, spanStartIndex + previousNoteLengths, noteSize);
                        synthesizer.Render(leftSpan, rightSpan);

                        previousNoteLengths += noteSize;
                    }
                }
            }
        }
        private (float[] Left, float[] Right) MixVocals(Score score, in float[] left, in float[] right, int sampleRate)
        {
            byte[] bytes = Synth.ConvertChannels(left, right);
            using MemoryStream memoryStream = new(bytes);
            WaveFormat waveFormat = new(sampleRate, Synth.BitsPerSample, 2);
            using RawSourceWaveStream waveStream = new(memoryStream, waveFormat);

            using AudioFileReader audioReader = new(@"filepath")
            {
                Volume = 1.0f,
            };

            int sampleCount = left.Length;
            float[] buffer = new float[sampleCount];
            // MixingSampleProvider mixer = new(new[] { waveStream.ToSampleProvider(), ConvertWaveFormat(waveFormat, audioReader).ToSampleProvider() });
            MixingSampleProvider mixer = new(new[] { waveStream.ToSampleProvider(), audioReader });
            int read = mixer.Read(buffer, 0, sampleCount);

            return Synth.SplitChannels(buffer);
        }
        #endregion

        #region Helpers
        private static IWaveProvider ConvertWaveFormat(WaveFormat targetWaveFormat, AudioFileReader audioReader)
        {
            if (audioReader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                //WaveFloatTo16Provider pcmConverter = new(audioReader);
                //return new WaveFormatConversionProvider(targetWaveFormat, pcmConverter);
                return new WaveFloatTo16Provider(audioReader);
            }
            else if (audioReader.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                //using WaveFormatConversionStream sampleRateConverter = new(new WaveFormat(waveFormat.SampleRate, audioReader.WaveFormat.BitsPerSample, audioReader.WaveFormat.Channels), audioReader);
                //using WaveFormatConversionStream bitDepthConverter = new(new WaveFormat(waveFormat.SampleRate, waveFormat.BitsPerSample, audioReader.WaveFormat.Channels), sampleRateConverter);
                //using WaveFormatConversionStream channelsConverter = new(waveFormat, bitDepthConverter);
                return new WaveFormatConversionStream(targetWaveFormat, audioReader);
            }
            throw new ApplicationException("Unexpected format.");
        }
        #endregion
    }
}
