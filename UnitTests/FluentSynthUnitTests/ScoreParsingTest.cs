using FluentSynth;

namespace FluentSynthUnitTests
{
    public class ScoreParsingTest
    {
        [Fact]
        public void MultiInstrumentScoreShouldAllowUnevenInstrumentGroups()
        {
            Score score = MusicalScoreParser.ParseCompleteScoreMultipleInstruments("""
                Mode: Multi-Instrument
                # Tempo 120
                # Time Signature 4/4
                (120) 4/4

                Piano 1:Piano [C C G G] [A A G/2] [F F E E] [D D C/2]
                Piano 1:Piano [G G F F] [E E D/2] [G G F F] [E E D/2]
                Piano 1:Piano [C C G G] [A A G/2] [F F E E] [D D C/2]

                Piano 2:Guitar [C C G G] [A A G/2] [F F E E] [D D C/2]
                Piano 2:Guitar [G G F F] [E E D/2] [G G F F] [E E D/2]
                Piano 3:Piano [C C G G] [A A G/2] [F F E E] [D D C/2]
                """);
            Assert.Equal(3, score.Measures[0].Sections.Length);
            Assert.Equal(2, score.Measures[4].Sections.Length);
            Assert.Equal(1, score.Measures[8].Sections.Length);
        }

        [Fact]
        public void MultiInstrumentScoreShouldAllowDefaultGroupNames()
        {
            Score score = MusicalScoreParser.ParseCompleteScoreMultipleInstruments("""
                Mode: Multi-Instrument
                # Tempo 120
                # Time Signature 4/4
                (120) 4/4

                Piano [C C G G] [A A G/2] [F F E E] [D D C/2]
                Piano [G G F F] [E E D/2] [G G F F] [E E D/2]
                Piano [C C G G] [A A G/2] [F F E E] [D D C/2]

                Guitar [C C G G] [A A G/2] [F F E E] [D D C/2]
                Guitar [G G F F] [E E D/2] [G G F F] [E E D/2]
                """);
            Assert.Equal(2, score.Measures[0].Sections.Length);
            Assert.Equal(1, score.Measures[8].Sections.Length);

            Assert.Equal("Piano", score.Measures.First().Sections.First().GroupName);
            Assert.Equal("Guitar", score.Measures.First().Sections.Last().GroupName);
        }

        [Fact]
        public void MultiInstrumentScoreShouldAllowInstrumentGroupsWithSameName()
        {
            Score score = MusicalScoreParser.ParseCompleteScoreMultipleInstruments("""
                Mode: Multi-Instrument
                # Tempo 120
                # Time Signature 4/4
                (120) 4/4

                Piano 1:Piano [C C G G] [A A G/2] [F F E E] [D D C/2]
                Piano 1:Piano [G G F F] [E E D/2] [G G F F] [E E D/2]

                Piano 2:Guitar [C C G G] [A A G/2] [F F E E] [D D C/2]
                Piano 2:Piano [G G F F] [E E D/2] [G G F F] [E E D/2]
                """);
            Assert.Equal(3, score.Measures[0].Sections.Length);
            Assert.Equal(1, score.Measures[4].Sections.Length);
        }

        [Fact]
        public void MultiInstrumentScoreShouldRecognizeVocals()
        {
            Score score = MusicalScoreParser.ParseCompleteScoreMultipleInstruments("""
                Mode: Multi-Instrument
                # Tempo 120
                # Time Signature 4/4
                (120) 4/4

                V1: Vocal1.wav
                V2: Vocal2.wav

                Piano [C C G G]
                Vocal [_ V1 _ V2]
                """);
            Assert.Equal(2, score.Measures[0].Sections.Length);
            Assert.Equal("Vocal", score.Measures[0].Sections.Last().GroupName);
            Assert.NotNull(score.Vocals);
            Assert.Single(score.Measures[0].Sections.Last().Notes.Last().Pitches);
            Assert.Equal("Vocal2.wav", score.Vocals[score.Measures[0].Sections.Last().Notes.Last().Pitches.First().VocalName]);
        }
    }
}