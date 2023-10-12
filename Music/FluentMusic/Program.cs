using FluentSynth;

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
            if (args.Length == 0 || args.FirstOrDefault() == "--help")
                PrintHelp();
            else if (args.Length == 1)
            {
                string soundFontFilePath = Path.GetFullPath(args.First());
                if (File.Exists(soundFontFilePath) && Path.GetExtension(soundFontFilePath) == ".sf2")
                    REPL(soundFontFilePath);
            }
            else if (args.Length == 2)
            {
                string soundFontFilePath = Path.GetFullPath(args.First());
                string fsmnFilePath = Path.GetFullPath(args.Last());
                if (File.Exists(soundFontFilePath) && Path.GetExtension(soundFontFilePath) == ".sf2")
                {
                    new Synth(soundFontFilePath).Play(File.ReadAllText(fsmnFilePath), out int duration);
                    Thread.Sleep(duration);
                }
            }
        }
        #endregion

        #region Routines
        private static void REPL(string soundFontFilePath)
        {
            Console.WriteLine("""
                Welcoe to Fluent Music, powered by Fluent Synth.
                ? Start typing notes or enter complete scores for playback.
                - Use `sample` to play some sample music.
                - Use `save <File Path>` to save history to a file.
                - Use `stop` to stop last playback.
                - Use `play <File Path>` to play from file.
                """);

            string fileName = Path.GetFileNameWithoutExtension(soundFontFilePath);
            string extension = Path.GetExtension(soundFontFilePath).TrimStart('.').ToUpper();
            Console.WriteLine($"{fileName} ({extension})");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                try
                {
                    new Synth(soundFontFilePath).Play(input, out _);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        public static void PrintHelp()
        {
            Console.WriteLine("""
                Fluent Music v0.1 Play music using score.
                --help: Print this help information.
                <Sound Font File>: Start REPL.
                """);
        }
        #endregion
    }
}