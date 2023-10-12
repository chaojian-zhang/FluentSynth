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
    }
}