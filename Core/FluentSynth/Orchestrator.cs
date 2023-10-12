using MeltySynth;

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
            Synthesizer synthesizer = new(SoundFontFilePath, SampleRate);

            double measureSizeInSeconds = (double)score.BeatsPerMeasure / score.BPM * 60;
            double totalSeconds = (int)Math.Ceiling(score.Measures.Length * measureSizeInSeconds);
            int measureSizeInFloats = (int)(measureSizeInSeconds * SampleRate);  // TODO: May have alignment problem
            int beatSizeInFloats = measureSizeInFloats / score.BeatsPerMeasure;

            // The output buffer
            int totalSamples = (int)(totalSeconds * SampleRate);  // TODO: May have alignment problem
            left = new float[totalSamples];
            right = new float[totalSamples];
            for (int m = 0; m < score.Measures.Length; m++)
            {
                Measure measure = score.Measures[m];
                int spanStartIndex = (int)(m * measureSizeInSeconds * SampleRate); // TODO: May have alignment problem

                synthesizer.NoteOffAll(true);

                for (int channel = 0; channel < measure.Sections.Length; channel++)
                {
                    // A channel is an independent path over which messages travel to their destination. There are 16 channels per MIDI device. A track in your sequencer program plays one instrument over a single channel. The MIDI messages in the track find their way to the instrument over that channel.
                    MeasureSection section = measure.Sections[channel];
                    synthesizer.ProcessMidiMessage(channel, 0xC0, section.MIDIInstrument, 0); // Send a program change command (0xC0) to the synthesizer in order to change instrument

                    int previousNoteLengths = 0;
                    for (int b = 0; b < section.Notes.Length; b++)
                    {
                        Note note = section.Notes[b];
                        foreach (int n in note.Notes)
                        {
                            if (n > 0)
                                synthesizer.NoteOn(channel, n, note.Velocity); // Signature: Channel (0 for both), key, velocity
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

            durationInMilliseconds = (int)(totalSeconds * 1000);
        }
        #endregion
    }
}
