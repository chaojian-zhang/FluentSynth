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

                // Stop all previous notes
                synthesizer.NoteOffAll(true);
                // Update instruments
                for (int c = 0; c < measure.Sections.Length; c++)
                {
                    MeasureSection channel = measure.Sections[c];
                    // Send a program change command (0xC0) to the synthesizer in order to change instrument
                    synthesizer.ProcessMidiMessage(c, 0xC0, channel.MIDIInstrument, 0);
                }

                foreach ((int SamplesElapsed, int Duration, (int Channel, Note Note)[] NoteChanges) in EnumerateNoteChanges(score, SampleRate, measure))
                {
                    // Perform new actions
                    foreach ((int Channel, Note Note) in NoteChanges)
                    {
                        foreach (NotePitch pitch in Note.Pitches)
                        {
                            // Typically we just have On and Pause notes, and no off event is needed
                            synthesizer.NoteOffAll(Channel, false); // With release
                            if (pitch.Pitch != Synth.StopNote)
                                // Signature: Channel (0 for both), key, velocity
                                synthesizer.NoteOn(Channel, pitch.Pitch, Note.Velocity);
                        }
                    }

                    // Render out the waveform up to the point of action                        
                    Span<float> leftSpan = new(left, spanStartIndex + SamplesElapsed, Duration);
                    Span<float> rightSpan = new(right, spanStartIndex + SamplesElapsed, Duration);
                    synthesizer.Render(leftSpan, rightSpan);
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
                            // The only non-vocal notes in a vocal track are the pauses
                            if (n.Pitch == Synth.StopNote)
                                continue; // Skip pauses
                            else
                            {
                                AudioFileReader reader = new AudioFileReader(score.Vocals[n.VocalName]);
                                int audioLength = (int)reader.Length;
                                int clipLength = Math.Min(audioLength, score.GetTotalSamples(SampleRate) - spanStartIndex - previousNoteLengths);
                                Span<float> leftSpan = new(left, spanStartIndex + previousNoteLengths, clipLength);
                                Span<float> rightSpan = new(right, spanStartIndex + previousNoteLengths, clipLength);

                                MixInAudio(leftSpan, rightSpan, sampleRate, reader);
                            }
                        }

                        int noteSize = score.GetBeatSizeInFloats(sampleRate) * (int)note.GetBeatCount(score.BeatSize);
                        previousNoteLengths += noteSize;
                    }
                }
            }
        }
        #endregion

        #region Subroutines
        private static void MixInAudio(Span<float> left, Span<float> right, int sampleRate, AudioFileReader mixin)
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
        private static IEnumerable<(int SamplesElapsed, int Duration, (int Channel, Note Note)[] NoteChanges)> EnumerateNoteChanges(Score score, int sampleRate, Measure measure)
        {
            // We are almost there with our current multiple-section (channel) per measure structure
            // To play Synthesizer as an instrument, we just need to figure out when to press and release each key, for each channel
            // And it's inherently a time-based approach; Or more precisely, an action-sequence based approach
            // We just need to divide all the actions into sequences and render each fragment at the time of all actions

            // Find smallest time unit in all channels
            int smallestUnit = measure.Sections
                .Where(s => s.MIDIInstrument != Synth.Vocal) // Don't handle vocals in MIDI engine
                .Max(s => s.Notes.Max(n => n.Duration));
            double scaler = score.BeatsPerMeasure / score.BeatSize;
            int quantiles = (int)(smallestUnit * scaler);
            // This list represents all the discret actions needed to represent all the note changes within this measure
            List<(int Channel, Note Note)>[] actions = Enumerable.Range(0, quantiles)
                .Select(i => new List<(int Channel, Note Note)>())
                .ToArray();

            // Enumerate and gather all actions: we try to insert the notes into approriate time slots in the pre-allocated action sequence
            for (int c = 0; c < measure.Sections.Length; c++)
            {
                MeasureSection channel = measure.Sections[c];

                int previousNoteSlotsCounts = 0;
                foreach (Note note in channel.Notes)
                {
                    actions[previousNoteSlotsCounts].Add((c, note));

                    previousNoteSlotsCounts += (int)note.GetBeatCount(smallestUnit);
                }
            }

            // Provide enumeration
            int duration = score.GetMeasureSizeInFloats(sampleRate) / actions.Length;
            for (int i = 0; i < actions.Length; i++)
            {
                int samplesElapsed = i * duration;
                yield return (samplesElapsed, duration, actions[i].ToArray());
            }
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
