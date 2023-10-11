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
        public void Orchestrate(Score score, out float[] left, out float[] right)
        {
            Synthesizer synthesizer = new(SoundFontFilePath, SampleRate);

            double measureSizeInSeconds = (double)score.BeatsPerMeasure / score.BPM * 60;
            int totalSeconds = (int)Math.Ceiling(score.Measures.Length * measureSizeInSeconds);  // TODO: May have alignment problem
            int measureSizeInFloats = (int)(measureSizeInSeconds * SampleRate);  // TODO: May have alignment problem
            int beatSizeInFloats = measureSizeInFloats / score.BeatsPerMeasure;

            // The output buffer
            int totalSamples = totalSeconds * SampleRate;
            left = new float[totalSamples];
            right = new float[totalSamples];
            for (int m = 0; m < score.Measures.Length; m++)
            {
                Measure measure = score.Measures[m];
                int spanStartIndex = (int)(m * measureSizeInSeconds * SampleRate); // TODO: May have alignment problem

                foreach (MeasureSection section in measure.Sections)
                {
                    synthesizer.NoteOffAll(true);
                    synthesizer.ProcessMidiMessage(0, 0xC0, section.MIDIInstrument, 0); // Send a program change command (0xC0) to the synthesizer in order to change instrument

                    for (int b = 0; b < section.Beats.Length; b++)
                    {
                        Beat beat = section.Beats[b];
                        foreach (int note in beat.Notes)
                        {
                            if (note > 0)
                                synthesizer.NoteOn(0, note, beat.Velocity); // Signature: Channel (0 for both), key, velocity
                            else
                                synthesizer.NoteOffAll(false); // With release
                        }

                        Span<float> leftSpan = new Span<float>(left, spanStartIndex + b * beatSizeInFloats, beatSizeInFloats);
                        Span<float> rightSpan = new Span<float>(right, spanStartIndex + b * beatSizeInFloats, beatSizeInFloats);
                        synthesizer.Render(leftSpan, rightSpan);
                    }
                }
            }
        }
        #endregion
    }
}
