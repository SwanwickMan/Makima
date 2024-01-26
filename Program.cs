using System;
using System.Speech.Recognition;
using System.IO;

namespace Makima
{
    class Makima 
    {
        public Dictionary<string, string> commands;

        public Makima()
        {
            commands = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader("Commands.txt"))
            {
                String[] content = sr.ReadToEnd().Split("\n");
                foreach (String line in content)
                {
                    if (!line.StartsWith("//"))
                    {
                        string[] entry = line.Split("|");
                        if (entry.Length == 2)
                        {
                            commands.Add(entry[0], entry[1]);
                        }
                        else
                        {
                            Console.WriteLine("bad form command: ",line);
                        }
                    }
                }
            }

        }


        static void Main(string[] args)
        {

            Choices c = new Choices();
            var gb = new GrammarBuilder(c);
            var customGrammar = new Grammar(gb);

            // Create an in-process speech recognizer for the en-US locale.  
            using (
            SpeechRecognitionEngine recognizer =
              new SpeechRecognitionEngine(
                new System.Globalization.CultureInfo("en-UK")))
            {

                // Create and load a dictation grammar.  
                recognizer.LoadGrammar(new DictationGrammar());
                recognizer.LoadGrammar(customGrammar);
                recognizer.MaxAlternates = 10;

                // Add a handler for the speech recognized event.  
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                // Configure input to the speech recognizer.  
                recognizer.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.  
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Keep the console window open.  
                while (true)
                {
                    Console.ReadLine();
                }
            }
        }

        // Handle the SpeechRecognized event.  
        static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized text: " + e.Result.Text);
            var x = e.Result.Alternates.ToList();
            foreach (RecognizedPhrase phrase in x)
            {
                Console.WriteLine("      ({0}) {1}", phrase.Confidence, phrase.Text);
            }
        }
    }
}