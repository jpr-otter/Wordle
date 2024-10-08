using Newtonsoft.Json.Linq;


namespace Wordle
{
    internal class Program
    {

        // TO-DO:
        /*
         * 
         * Console optisch anpassen!
         * Methoden auslagern!
         * 
         *           
         * random word api https://random-word-api.herokuapp.com/home
         * 
         */

        public static List<string> FiveLetterWords =>
        [
                "apple", "grape", "mango", "peach", "plumb",
                "berry", "melon", "lemon", "cherry", "beach",
                "fruit", "plumb", "wheat", "sweet", "spice",
                "fence", "chair", "table", "glass", "stone",
                "knife", "track", "brave", "storm", "dance",
                "train", "blues", "laugh", "smile", "light",
                "water", "music", "peace", "world", "style",
                "image", "value", "chair", "point", "share",
                "candy", "sugar", "honey", "piano", "music",
                "spoon", "plate", "glass", "apple", "pearl",
                "hitch", "cloud", "dream", "vivid", "grace",
                "crane", "brick", "flame", "quilt", "laser",
                "brisk", "spine", "frost", "flare", "bloom"
        ];

        public static int MaxLength => 5;
        public static int MaxTries => 5;
        public static List<char> Alphabet = [.. "abcdefghijklmnopqrstuvwxyz".ToCharArray()];

        static async Task Main()
        {
            Console.WriteLine("Welcome to wordle!" +
                              $"\nGuess the {MaxLength} letter word. You have {MaxTries} tries" +
                              "\n");

            string wordOfTheDay = PickRandomWord();
            string? guessedWord = string.Empty;
            bool solved = false;
            int tryCounter = 0;
            List<char> triedLetters = [];
            Dictionary<char, ConsoleColor> letterStatus = [];

            foreach (char letter in Alphabet)
            {
                letterStatus[letter] = ConsoleColor.Black;
            }

            while (!solved && tryCounter < MaxTries)
            {
                bool correctGuessedWordLength = false;
                bool isValidEnglishWord = false;

                while (!correctGuessedWordLength || !isValidEnglishWord)
                {
                    guessedWord = Console.ReadLine();

                    bool isEmpty = string.IsNullOrEmpty(guessedWord);
                    if (isEmpty)
                    {
                        Console.WriteLine($"\nPlease type something.");
                        continue;
                    }

                    bool isValid = await IsValidWord(guessedWord!);
                    bool isMixed = guessedWord!.Any(char.IsLetter) && guessedWord!.Any(char.IsNumber);
                    bool isNumeric = int.TryParse(guessedWord, out _);

                    if (isValid) isValidEnglishWord = true;
                    else Console.WriteLine("\nYour input is not a valid English word. Try again.");
                    if (isNumeric || isMixed) Console.WriteLine($"\nOnly letters please.");
                    if (guessedWord?.Length == MaxLength && !isNumeric && !isMixed) correctGuessedWordLength = true;
                    else if (guessedWord?.Length > MaxLength && !isNumeric && !isMixed) Console.WriteLine($"\nWord is too long. Only {MaxLength} letters");
                    else if (guessedWord?.Length < MaxLength && !isNumeric && !isMixed) Console.WriteLine($"\nWord is too short. {MaxLength} letters please.");
                }

                triedLetters = [.. guessedWord];

                Console.WriteLine();

                for (int i = 0; i < wordOfTheDay.Length; i++)
                {
                    char guessedLetter = guessedWord![i];

                    if (guessedLetter == wordOfTheDay[i])
                    {
                        letterStatus[guessedLetter] = ConsoleColor.DarkGreen;
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.Write(guessedWord[i]);
                        Console.ResetColor();
                    }
                    else if (guessedLetter != wordOfTheDay[i] && wordOfTheDay.Contains(guessedWord[i]))
                    {
                        letterStatus[guessedLetter] = ConsoleColor.DarkYellow;
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.Write(guessedLetter);
                        Console.ResetColor();
                    }
                    else if (guessedLetter != wordOfTheDay[i])
                    {
                        letterStatus[guessedLetter] = ConsoleColor.DarkRed;
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.Write(guessedLetter);
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();

                DisplayAlphabet(letterStatus);

                tryCounter++;

                if (guessedWord == wordOfTheDay)
                {
                    solved = true;
                    Console.Write("\nYou found the word!");
                }

                Console.WriteLine();
            }

            if (tryCounter < MaxTries)
            {
                Console.WriteLine("\nYou exceeded the number of tries");
                Console.WriteLine($"\nThe word we were looking for is: {wordOfTheDay}");
            }
        }

        private static void DisplayAlphabet(Dictionary<char, ConsoleColor> letterStatus)
        {
            Console.WriteLine();
            foreach (var letter in letterStatus.Keys)
            {
                Console.BackgroundColor = letterStatus[letter];
                Console.Write($"{letter}");
                Console.ResetColor();
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        private static string PickRandomWord()
        {
            return FiveLetterWords[new Random().Next(FiveLetterWords.Count)];
        }

        public static async Task<bool> IsValidWord(string word)
        {
            //string apiKey = "";
            string apiUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";

            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return false;

                string responseBody = await response.Content.ReadAsStringAsync();
                JArray jsonArray = JArray.Parse(responseBody);

                return jsonArray.Count > 0 && jsonArray[0]["word"] != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}

