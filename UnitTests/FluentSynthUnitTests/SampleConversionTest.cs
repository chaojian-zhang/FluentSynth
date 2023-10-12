using FluentSynth;

namespace FluentSynthUnitTests
{
    public class SampleConversionTest
    {
        [Fact]
        public void DualChannelConversionShouldPreservePrecision()
        {
            // Populate two samples
            float[] left = Enumerable.Range(0, 9999).Select(i => (float)i / 9999).ToArray();
            float[] right = Enumerable.Range(0, 9999).Reverse().Select(i => (float)i / 9999).ToArray();

            // Merge
            byte[] bytes = Synth.ConvertChannels(left, right);
            (float[] Left, float[] Right) result = Synth.SplitChannels(bytes);

            // Assert
            int sampleCount = left.Length;
            for (int i = 0; i < sampleCount; i++)
            {
                Assert.True(Math.Abs(left[i] - result.Left[i]) < 1e-4);
                Assert.True(Math.Abs(right[i] - result.Right[i]) < 1e-4);
            }
        }
    }
}