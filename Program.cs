using System;
using System.Speech.Recognition;
using System.IO;
using System.Diagnostics;

namespace Makima
{
    class Makima 
    {
        private Dictionary<string, string> commands;
        private SpeechRecognitionEngine recognizer;
        private Boolean debug = false;

        private void runCommand(String voiceCommand)
        {
            if (voiceCommand is null) { return; }
            String command = commands.GetValueOrDefault(voiceCommand, null);
            Console.WriteLine(command);
            if (command is not null) 
            { 
                //Execute command
                Process cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/c {command}";
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.Start();
                string output = cmd.StandardOutput.ReadToEnd()!;
                cmd.WaitForExit();
            
            }
        }
        private void debugOutput(SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized text: " + e.Result.Text);
            foreach (RecognizedPhrase phrase in e.Result.Alternates.ToList())
            {
                Console.WriteLine("      ({0}) {1}", phrase.Confidence, phrase.Text);
            }
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (debug) { debugOutput(e); }
            runCommand(e.Result.Text);
        }

        public Makima()
        {
            Choices c = new Choices();
            commands = new Dictionary<string, string>();
            String file = "Commands.txt";
            if (!File.Exists(file)) { File.Create(file); }
            using (StreamReader sr = new StreamReader(file))
            {
                String[] content = sr.ReadToEnd().Split("\n");
                foreach (String line in content)
                {
                    if (!line.StartsWith("//"))
                    {
                        string[] entry = line.Split("|");
                        if (entry.Length == 2)
                        {
                            Console.WriteLine("found: " + entry[0] + " -> " + entry[1]);
                            commands.Add(entry[0], entry[1]);
                            c.Add(entry[0]);
                        }
                        else
                        {
                            Console.WriteLine("bad form command: ",line);
                        }
                    }
                }
            }

            var gb = new GrammarBuilder(c);
            var commandsGrammar = new Grammar(gb);

            recognizer = new SpeechRecognitionEngine(
                new System.Globalization.CultureInfo("en-UK"));
            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.LoadGrammar(commandsGrammar);
            recognizer.MaxAlternates = 10;
            recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }
        public void switchDebug() { debug = !debug; Console.WriteLine("debug: " + debug.ToString()); }

        static void Main(string[] args)
        {
            Makima makima = new Makima();
            while (true) { Console.ReadLine(); makima.switchDebug(); }
        }
    }
}