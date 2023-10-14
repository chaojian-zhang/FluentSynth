using FluentSynth;
using NAudio.Wave;

namespace FluentMusic
{
    public struct CommandLineOptions
    {
        public string SoundFontFile { get; set; }
    }
    public static class Program
    {
        #region Main
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                Console.ReadLine(); // Block on input so the console window doesn't just quit when launched from file explorer
            }
            else if (args.FirstOrDefault() == "--help")
                PrintHelp();
            else
            {
                switch (args.First().ToLower())
                {
                    case "render":
                        RenderMusic(args.Skip(1).ToArray());
                        break;
                    case "play":
                        PlayMusic(args.Skip(1).ToArray());
                        break;
                    default:
                        DefaultHandling(args);
                        break;
                }
            }
        }
        private static void DefaultHandling(string[] args)
        {
            if (args.Length == 1)
            {
                string soundFontFilePath = Path.GetFullPath(args.First());
                if (File.Exists(soundFontFilePath) && Path.GetExtension(soundFontFilePath) == ".sf2")
                    REPL(soundFontFilePath);
            }
            else if (args.Length == 2)
                PlayMusic(args);
        }
        private static void PlayMusic(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Not enough arguments. Expect: <Sound File> <Score/MIDI>");
                return;
            }

            string soundFontFilePath = Path.GetFullPath(args.First());
            string inputFilePath = Path.GetFullPath(args.Last());
            if (!File.Exists(soundFontFilePath))
            {
                Console.WriteLine($"SoundFond file doesn't exist: {soundFontFilePath}");
                return;
            }
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Input file doesn't exist: {inputFilePath}");
                return;
            }

            if (Path.GetExtension(soundFontFilePath) == ".sf2")
            {
                CurrentPlaying = PlayMediaFile(soundFontFilePath, inputFilePath, out int duration);
                Thread.Sleep(duration);
            }
        }
        private static void RenderMusic(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Not enough arguments. Expect: <Sound File> <Score/MIDI> <Output>");
                return;
            }

            string soundFontFilePath = Path.GetFullPath(args.First());
            string inputFilePath = Path.GetFullPath(args[1]);
            string outputFilePath = Path.GetFullPath(args.Last());
            if (!File.Exists(soundFontFilePath))
            {
                Console.WriteLine($"SoundFond file doesn't exist: {soundFontFilePath}");
                return;
            }
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Input file doesn't exist: {inputFilePath}");
                return;
            }

            if (Path.GetExtension(soundFontFilePath) == ".sf2")
                RenderMediaFile(soundFontFilePath, inputFilePath, outputFilePath);
        }
        #endregion

        #region Routines
        private static WaveOutEvent PlayMediaFile(string soundFontFilePath, string inputFilePath, out int duration)
        {
            Console.WriteLine($"Play {Path.GetFileNameWithoutExtension(inputFilePath)}...");
            switch (Path.GetExtension(inputFilePath))
            {
                case ".fs":
                    return new Synth(soundFontFilePath).Play(File.ReadAllText(inputFilePath), out duration);
                case ".mid":
                    return new Synth(soundFontFilePath).PlayMIDIFile(inputFilePath, out duration);
                default:
                    duration = 0;
                    return null;
            }
        }
        private static void RenderMediaFile(string soundFontFilePath, string inputFilePath, string outputFilePath)
        {
            Console.WriteLine($"Render {Path.GetFileNameWithoutExtension(inputFilePath)}...");
            switch (Path.GetExtension(inputFilePath))
            {
                case ".fs":
                    new Synth(soundFontFilePath).Render(File.ReadAllText(inputFilePath), outputFilePath);
                    break;
                case ".mid":
                    new Synth(soundFontFilePath).RenderMIDIFile(inputFilePath, outputFilePath);
                    break;
                default:
                    break;
            }
            Console.WriteLine($"Output saved to {outputFilePath}");
            Console.WriteLine($"Done.");
        }
        private static WaveOutEvent CurrentPlaying;
        private static void REPL(string soundFontFilePath)
        {
            Console.WriteLine("""
                Welcoe to Fluent Music, powered by Fluent Synth.
                ? Start typing notes or enter complete scores for playback. Type `exit` to quit.
                - Use `sample` to play some sample music.
                - Use `save <File Path>` to save history to a file.
                - Use `stop` to stop last playback.
                - Use `play <File Path>` to play from file.
                """);

            string fileName = Path.GetFileNameWithoutExtension(soundFontFilePath);
            string extension = Path.GetExtension(soundFontFilePath).TrimStart('.').ToUpper();
            Console.WriteLine($"Now playing using: {fileName} ({extension})");
            List<string> history = new List<string>();
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (input == "exit")
                    break;
                else if (input == "sample")
                    CurrentPlaying = new Synth(soundFontFilePath).Play("[C C G G] [A A G/2] [F F E E] [D D C/2]", out _);
                else if (input == "stop")
                    CurrentPlaying?.Stop();
                else if (input.StartsWith("save "))
                    File.WriteAllLines(input["save ".Length..].Trim().Trim('"'), history);
                else if (input.StartsWith("play "))
                    CurrentPlaying = PlayMediaFile(soundFontFilePath, input["play ".Length..].Trim().Trim('"'), out _);
                else
                {
                    // Play melodies
                    try
                    {
                        history.Add(input);
                        new Synth(soundFontFilePath).Play(input, out _);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
        public static void PrintHelp()
        {
            // remark-cz: Fluent Music version goes with Fluent Synth core version.
            Console.WriteLine("""
                Fluent Music v0.2 Play music using score.
                --help: Print this help information.
                <Sound Font File>: Start REPL.

                Subcommands:
                - render
                - play
                """);
        }
        #endregion
    }
}