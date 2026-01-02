using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Arcanum.Core.Utils.LanguageEngine;

public static class NameGenerator
{
    public static readonly string WordsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NameGeneratorWords");

    public class WeightedList<T>
    {
        List<T> Elements;
        List<int> Weights;
        int TotalWeights = 0;
        Random Random = new Random();
        public WeightedList(int Capacity = 1024)
        {
            Elements = new List<T>(Capacity);
            Weights = new List<int>(Capacity);
        }
        public void Add(T element, int weight)
        {
            if (weight <= 0)
            {
                return;
            }
            Elements.Add(element);
            Weights.Add(weight);
            TotalWeights += weight;
        }
        public bool Remove(T element)
        {
            int Index = Elements.IndexOf(element);
            if (Index == -1)
            {
                return false;
            }
            Elements.RemoveAt(Index);
            TotalWeights -= Weights[Index];
            Weights.RemoveAt(Index);
            return true;
        }
        public T? GetRandomElement()
        {
            if (TotalWeights == 0)
            {
                return default;
            }
            int N = Random.Next(TotalWeights);
            int Counted = 0;
            for (int i = 0; i < Elements.Count; i++)
            {
                Counted += Weights[i];
                if (Counted > N)
                {
                    return Elements[i];
                }
            }
            return default;
        }
    }

    public static List<Language> Languages = new List<Language>();

    public class Language
    {
        string Name = "";
        int TotalWords = 0;
        int UnusualLetterThreshold = 0;
        Dictionary<string, int>[] Parts = new Dictionary<string, int>[3]
        {
        new Dictionary<string, int>(10000),
        new Dictionary<string, int>(10000),
        new Dictionary<string, int>(10000)
        };
        Dictionary<string, Dictionary<string, int>> NextLetterFrequency = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<string, int>> SecondLetterFrequency = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<string, int>> ThirdLetterFrequency = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, int> WordBeginnings = new Dictionary<string, int>(1000);
        Dictionary<string, int> WordEndings = new Dictionary<string, int>(1000);
        Dictionary<string, int> FirstLetterFrequency = new Dictionary<string, int>();
        Dictionary<string, int> LastLetterFrequency = new Dictionary<string, int>();

        List<string> SimpleSyllables = new List<string>();
        bool UsesSimpleSyllables = false;

        bool AllowTripleRepetition = false;
        public Language(string Name)
        {
            this.Name = Name;
        }

        public string GetName()
        {
            return Name;
        }

        public string GenerateWord(int ApproximateDesiredLength)
        {
            Random random = new Random();
            string Word = "";
            List<string> PartsUsed = new List<string>(10);
            {
                bool Ending = false;
                while (Ending == false)
                {
                    if (Word.Length > ApproximateDesiredLength - 3)
                    {
                        Ending = true;
                    }
                    string Part = GetPart(Word, Ending, PartsUsed);
                    if (Part == "" || Part == null)
                    {
                        break;
                    }
                    Word += Part;
                }
            }
            return Word;
        }

        private string GetPart(string ExistingWord, bool Ending, List<string> PartsUsed)
        {
            if (UsesSimpleSyllables)
            {
                if (SimpleSyllables.Count == 0)
                {
                    return "";
                }
                Random r = new Random();
                return SimpleSyllables[r.Next(SimpleSyllables.Count)];
            }

            string LastLetter = ExistingWord.Length > 0 ? ExistingWord.Last().ToString() : "";
            string SecondToLastLetter = ExistingWord.Length > 1 ? ExistingWord.Substring(ExistingWord.Length - 2, 1) : "";
            string ThirdToLastLetter = ExistingWord.Length > 2 ? ExistingWord.Substring(ExistingWord.Length - 3, 1) : "";
            WeightedList<string> Parts = new WeightedList<string>();
            foreach (Dictionary<string, int> PartsDictionary in this.Parts)
            {
                foreach (string part in PartsDictionary.Keys)
                {
                    string FirstLetter = part.Substring(0, 1);
                    string SecondLetter = part.Length > 1 ? part.Substring(1, 1) : "";
                    string ThirdLetter = part.Length > 2 ? part.Substring(2, 1) : "";

                    if (AllowTripleRepetition == false)
                    {
                        if (SecondToLastLetter == LastLetter && LastLetter == FirstLetter)
                        {
                            continue;
                        }
                        if (LastLetter == FirstLetter && FirstLetter == SecondLetter)
                        {
                            continue;
                        }
                    }

                    int Weight = PartsDictionary[part];
                    int Divisor = 1;
                    if (ExistingWord.Length > 0)
                    {
                        if (NextLetterFrequency.ContainsKey(LastLetter) == false || NextLetterFrequency[LastLetter].ContainsKey(FirstLetter) == false)
                        {
                            continue;
                        }
                        else
                        {
                            Weight += NextLetterFrequency[LastLetter][FirstLetter];
                        }
                        if (PartsUsed.Contains(part))
                        {
                            Divisor *= 3;
                        }
                        if (SecondToLastLetter != "")
                        {
                            if (SecondLetterFrequency.ContainsKey(SecondToLastLetter) == false ||
                                SecondLetterFrequency[SecondToLastLetter].ContainsKey(FirstLetter) == false)
                            {
                                Divisor *= 4;
                            }
                            else
                            {
                                Weight += SecondLetterFrequency[SecondToLastLetter][FirstLetter];
                            }

                            if (ThirdLetterFrequency.ContainsKey(SecondToLastLetter) == false ||
                                ThirdLetterFrequency[SecondToLastLetter].ContainsKey(SecondLetter) == false)
                            {
                                Divisor *= 2;
                            }
                            else
                            {
                                Weight += ThirdLetterFrequency[SecondToLastLetter][SecondLetter];
                            }

                        }

                        if (SecondLetterFrequency.ContainsKey(LastLetter) == false ||
                            SecondLetterFrequency[LastLetter].ContainsKey(SecondLetter) == false)
                        {
                            Divisor *= 4;
                        }
                        else
                        {
                            Weight += SecondLetterFrequency[LastLetter][SecondLetter];
                        }

                        if (ThirdLetter != "")
                        {
                            if (ThirdLetterFrequency.ContainsKey(LastLetter) == false ||
                            ThirdLetterFrequency[LastLetter].ContainsKey(ThirdLetter) == false)
                            {
                                Divisor *= 2;
                            }
                            else
                            {
                                Weight += ThirdLetterFrequency[LastLetter][ThirdLetter];
                            }
                        }

                        if (ThirdToLastLetter != "")
                        {
                            if (ThirdLetterFrequency.ContainsKey(ThirdToLastLetter) == false ||
                                ThirdLetterFrequency[ThirdToLastLetter].ContainsKey(FirstLetter) == false)
                            {
                                Divisor *= 2;
                            }
                            else
                            {
                                Weight += ThirdLetterFrequency[ThirdToLastLetter][FirstLetter];
                            }
                        }



                    }
                    else
                    {
                        if (WordBeginnings.ContainsKey(part) == false)
                        {
                            continue;
                        }
                        else
                        {
                            if (FirstLetterFrequency.ContainsKey(FirstLetter) == false)
                            {
                                continue;
                            }
                            if (FirstLetterFrequency[FirstLetter] < UnusualLetterThreshold)
                            {
                                continue;
                            }
                            Weight += WordBeginnings[part];
                        }
                    }

                    if (Ending)
                    {
                        if (WordEndings.ContainsKey(part) == false)
                        {
                            Divisor *= 10;
                        }
                        else
                        {
                            Weight += WordEndings[part];
                        }

                        string LastLetterOfPart = part.Substring(part.Length - 1);
                        if (LastLetterFrequency.ContainsKey(LastLetterOfPart) == false)
                        {
                            continue;
                        }
                        if (LastLetterFrequency[LastLetterOfPart] < UnusualLetterThreshold)
                        {
                            continue;
                        }
                    }

                    Weight /= Divisor;
                    Parts.Add(part, Weight);
                }
            }
            string Part = Parts.GetRandomElement() ?? "";
            PartsUsed.Add(Part);
            return Part;
        }

        public static string GenerateWord(List<Language> Languages, int ApproximateDesiredLength)
        {
            if (Languages.Count == 0)
            {
                return "";
            }

            Random random = new Random();
            string Word = "";
            List<string> PartsUsed = new List<string>(10);
            {
                bool Ending = false;
                while (Ending == false)
                {
                    Language language = Languages[random.Next(Languages.Count)];
                    if (Word.Length > ApproximateDesiredLength - 3)
                    {
                        Ending = true;
                    }
                    Word += language.GetPart(Word, Ending, PartsUsed);
                }
            }
            return Word;
        }

        public void LoadWords(string[] words)
        {
            foreach (Dictionary<string, int> PartDictionary in Parts)
            {
                PartDictionary.Clear();
            }
            NextLetterFrequency.Clear();
            SecondLetterFrequency.Clear();
            WordBeginnings.Clear();
            WordEndings.Clear();
            FirstLetterFrequency.Clear();
            LastLetterFrequency.Clear();
            TotalWords = 0;
            foreach (string line in words)
            {
                string word = line;
                if (word.Contains(' '))
                {
                    word = word.Split(' ')[0];
                }
                if (word.Length < 2)
                {
                    continue;
                }

                TotalWords++;

                for (int i = 2; i <= Math.Min(4, word.Length); i++)
                {
                    string beginning = word.Substring(0, i);
                    if (WordBeginnings.TryAdd(beginning, 1) == false)
                    {
                        WordBeginnings[beginning]++;
                    }

                    string ending = word.Substring(word.Length - i, i);
                    if (WordEndings.TryAdd(ending, 1) == false)
                    {
                        WordEndings[ending]++;
                    }
                }

                for (int letters = 2; letters < 5; letters++)
                {
                    Dictionary<string, int> PartsDictionary = Parts[letters - 2];

                    for (int i = 0; i <= word.Length - letters; i++)
                    {
                        string substring = word.Substring(i, letters);
                        if (substring.Length == 2)
                        {
                            if (substring[0] == substring[1])
                            {
                                continue;
                            }
                        }
                        if (PartsDictionary.TryAdd(substring, 1) == false)
                        {
                            PartsDictionary[substring]++;
                        }
                    }
                }

                for (int i = 0; i < word.Length - 1; i++)
                {
                    string letter = word.Substring(i, 1);
                    string nextLetter = word.Substring(i + 1, 1);
                    NextLetterFrequency.TryAdd(letter, new Dictionary<string, int>());
                    if (NextLetterFrequency[letter].TryAdd(nextLetter, 1) == false)
                    {
                        NextLetterFrequency[letter][nextLetter]++;
                    }
                }

                for (int i = 0; i < word.Length - 2; i++)
                {
                    string letter = word.Substring(i, 1);
                    string nextLetter = word.Substring(i + 2, 1);
                    SecondLetterFrequency.TryAdd(letter, new Dictionary<string, int>());
                    if (SecondLetterFrequency[letter].TryAdd(nextLetter, 1) == false)
                    {
                        SecondLetterFrequency[letter][nextLetter]++;
                    }
                }

                for (int i = 0; i < word.Length - 3; i++)
                {
                    string letter = word.Substring(i, 1);
                    string nextLetter = word.Substring(i + 3, 1);
                    ThirdLetterFrequency.TryAdd(letter, new Dictionary<string, int>());
                    if (ThirdLetterFrequency[letter].TryAdd(nextLetter, 1) == false)
                    {
                        ThirdLetterFrequency[letter][nextLetter]++;
                    }
                }

                string firstLetter = word.Substring(0, 1);
                if (FirstLetterFrequency.TryAdd(firstLetter, 1) == false)
                {
                    FirstLetterFrequency[firstLetter]++;
                }

                string lastLetter = word.Substring(word.Length - 1, 1);
                if (LastLetterFrequency.TryAdd(lastLetter, 1) == false)
                {
                    LastLetterFrequency[lastLetter]++;
                }
            }

            UnusualLetterThreshold = TotalWords / 500;
        }

        public void LoadSyllables(string[] syllables)
        {
            UsesSimpleSyllables = true;
            foreach (string line in syllables)
            {
                string syllable = line;
                if (syllable.Contains(' '))
                {
                    syllable = syllable.Split(' ')[0];
                }
                if (syllable.Length == 0)
                {
                    continue;
                }
                SimpleSyllables.Add(syllable);
            }
        }
    }

    public static void LoadLanguages()
    {
        Languages.Clear();

        if(Directory.Exists(WordsFolder) == false)
        {
            return;
        }

        foreach (string filePath in Directory.GetFiles(WordsFolder))
        {
            string fileName = Path.GetFileName(filePath);

            if (fileName.EndsWith(".txt") == false)
            {
                continue;
            }

            if (fileName.StartsWith("l_") == false && fileName.StartsWith("ls_") == false)
            {
                continue;
            }
            bool SyllableLanguage = fileName.StartsWith("ls_");
            string LanguageName = fileName.Substring(SyllableLanguage ? 3 : 2).Split('.')[0];
            Language language = new Language(LanguageName);

            if (SyllableLanguage)
            {
                language.LoadSyllables(File.ReadAllLines(fileName));
            }
            else
            {
                language.LoadWords(File.ReadAllLines(fileName));
            }
            Languages.Add(language);
        }
    }
}