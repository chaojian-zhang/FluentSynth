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
                string filePath = Path.GetFullPath(args.First());
                if (File.Exists(filePath) && Path.GetExtension(filePath) == ".sf2")
                    REPL(filePath);
            }
        }
        #endregion

        #region Routines
        private static void REPL(string soundFontFilePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(soundFontFilePath);
            string extension = Path.GetExtension(soundFontFilePath).TrimStart('.').ToUpper();
            Console.WriteLine($"{fileName} ({extension})");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                try
                {
                    new Synth(soundFontFilePath).Play(input);
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