using System.Reflection;

namespace Arcanum.Core.Utils.NameGenerator;

public static class NameGenerator
{
   static readonly Assembly Assembly = typeof(NameGenerator).Assembly;

   public static List<Language> Languages = new();

   public static void LoadLanguages()
   {
      Languages.Clear();

      foreach (var resource in Assembly.GetManifestResourceNames())
      {
         if (!resource.EndsWith(".lang", StringComparison.OrdinalIgnoreCase))
            continue;

         var parts = resource.Split('.');
         var file = parts[^2]; // l_english or ls_japanese

         if (!file.StartsWith("l_") && !file.StartsWith("ls_"))
            continue;

         var syllables = file.StartsWith("ls_");
         var languageName = file[(syllables ? 3 : 2)..];

         using var stream = Assembly.GetManifestResourceStream(resource)!;
         using var reader = new StreamReader(stream);
         var lines = reader.ReadToEnd()
                           .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

         var language = new Language(languageName);

         if (syllables)
            language.LoadSyllables(lines);
         else
            language.LoadWords(lines);

         Languages.Add(language);
      }
   }

   public class WeightedList<T>
   {
      private readonly List<T> _elements;
      private readonly Random _random = new();
      private readonly List<int> _weights;
      private int _totalWeights;

      public WeightedList(int capacity = 1024)
      {
         _elements = new(capacity);
         _weights = new(capacity);
      }

      public void Add(T element, int weight)
      {
         if (weight <= 0)
            return;

         _elements.Add(element);
         _weights.Add(weight);
         _totalWeights += weight;
      }

      public bool Remove(T element)
      {
         var index = _elements.IndexOf(element);
         if (index == -1)
            return false;

         _elements.RemoveAt(index);
         _totalWeights -= _weights[index];
         _weights.RemoveAt(index);
         return true;
      }

      public T? GetRandomElement()
      {
         if (_totalWeights == 0)
            return default;

         var n = _random.Next(_totalWeights);
         var counted = 0;
         for (var i = 0; i < _elements.Count; i++)
         {
            counted += _weights[i];
            if (counted > n)
               return _elements[i];
         }

         return default;
      }
   }

   public class Language
   {
      private readonly bool _allowTripleRepetition = false;
      private readonly Dictionary<string, int> _firstLetterFrequency = new();
      private readonly Dictionary<string, int> _lastLetterFrequency = new();
      private readonly Dictionary<string, Dictionary<string, int>> _nextLetterFrequency = new();

      private readonly Dictionary<string, int>[] _parts = new Dictionary<string, int>[3] { new(10000), new(10000), new(10000) };

      private readonly Dictionary<string, Dictionary<string, int>> _secondLetterFrequency = new();

      private readonly List<string> _simpleSyllables = new();
      private readonly Dictionary<string, Dictionary<string, int>> _thirdLetterFrequency = new();
      private readonly Dictionary<string, int> _wordBeginnings = new(1000);
      private readonly Dictionary<string, int> _wordEndings = new(1000);
      private int _totalWords;
      private int _unusualLetterThreshold;
      private bool _usesSimpleSyllables;

      public Language(string name) => Name = name;
      public string Name { get; }

      public string GetName() => Name;

      public string GenerateWord(int approximateDesiredLength)
      {
         var random = new Random();
         var word = "";
         var partsUsed = new List<string>(10);
         {
            var ending = false;
            while (!ending)
            {
               if (word.Length > approximateDesiredLength - 3)
                  ending = true;
               var part = GetPart(word, ending, partsUsed);
               if (part == "" || part == null)
                  break;

               word += part;
            }
         }
         return word;
      }

      private string GetPart(string existingWord, bool ending, List<string> partsUsed)
      {
         if (_usesSimpleSyllables)
         {
            if (_simpleSyllables.Count == 0)
               return "";

            var r = new Random();
            return _simpleSyllables[r.Next(_simpleSyllables.Count)];
         }

         var lastLetter = existingWord.Length > 0 ? existingWord.Last().ToString() : "";
         var secondToLastLetter = existingWord.Length > 1 ? existingWord.Substring(existingWord.Length - 2, 1) : "";
         var thirdToLastLetter = existingWord.Length > 2 ? existingWord.Substring(existingWord.Length - 3, 1) : "";
         var list = new WeightedList<string>();
         foreach (var partsDictionary in _parts)
         {
            foreach (var part in partsDictionary.Keys)
            {
               var firstLetter = part[..1];
               var secondLetter = part.Length > 1 ? part.Substring(1, 1) : "";
               var thirdLetter = part.Length > 2 ? part.Substring(2, 1) : "";

               if (!_allowTripleRepetition)
               {
                  if (secondToLastLetter == lastLetter && lastLetter == firstLetter)
                     continue;

                  if (lastLetter == firstLetter && firstLetter == secondLetter)
                     continue;
               }

               var weight = partsDictionary[part];
               var divisor = 1;
               if (existingWord.Length > 0)
               {
                  if (!_nextLetterFrequency.ContainsKey(lastLetter) || !_nextLetterFrequency[lastLetter].ContainsKey(firstLetter))
                     continue;

                  weight += _nextLetterFrequency[lastLetter][firstLetter];
                  if (partsUsed.Contains(part))
                     divisor *= 3;
                  if (secondToLastLetter != "")
                  {
                     if (!_secondLetterFrequency.ContainsKey(secondToLastLetter) ||
                         !_secondLetterFrequency[secondToLastLetter].ContainsKey(firstLetter))
                        divisor *= 4;
                     else
                        weight += _secondLetterFrequency[secondToLastLetter][firstLetter];

                     if (!_thirdLetterFrequency.ContainsKey(secondToLastLetter) ||
                         !_thirdLetterFrequency[secondToLastLetter].ContainsKey(secondLetter))
                        divisor *= 2;
                     else
                        weight += _thirdLetterFrequency[secondToLastLetter][secondLetter];
                  }

                  if (!_secondLetterFrequency.ContainsKey(lastLetter) ||
                      !_secondLetterFrequency[lastLetter].ContainsKey(secondLetter))
                     divisor *= 4;
                  else
                     weight += _secondLetterFrequency[lastLetter][secondLetter];

                  if (thirdLetter != "")
                  {
                     if (!_thirdLetterFrequency.ContainsKey(lastLetter) ||
                         !_thirdLetterFrequency[lastLetter].ContainsKey(thirdLetter))
                        divisor *= 2;
                     else
                        weight += _thirdLetterFrequency[lastLetter][thirdLetter];
                  }

                  if (thirdToLastLetter != "")
                  {
                     if (!_thirdLetterFrequency.ContainsKey(thirdToLastLetter) ||
                         !_thirdLetterFrequency[thirdToLastLetter].ContainsKey(firstLetter))
                        divisor *= 2;
                     else
                        weight += _thirdLetterFrequency[thirdToLastLetter][firstLetter];
                  }
               }
               else
               {
                  if (!_wordBeginnings.ContainsKey(part))
                     continue;

                  if (!_firstLetterFrequency.TryGetValue(firstLetter, out var value))
                     continue;

                  if (value < _unusualLetterThreshold)
                     continue;

                  weight += _wordBeginnings[part];
               }

               if (ending)
               {
                  if (!_wordEndings.TryGetValue(part, out var wordEnding))
                     divisor *= 10;
                  else
                     weight += wordEnding;

                  var lastLetterOfPart = part[^1..];
                  if (!_lastLetterFrequency.TryGetValue(lastLetterOfPart, out var value))
                     continue;

                  if (value < _unusualLetterThreshold)
                     continue;
               }

               weight /= divisor;
               list.Add(part, weight);
            }
         }

         var getPart = list.GetRandomElement() ?? "";
         partsUsed.Add(getPart);
         return getPart;
      }

      public static string GenerateWord(List<Language> languages, int approximateDesiredLength)
      {
         if (languages.Count == 0)
            return "";

         var random = new Random();
         var word = "";
         var partsUsed = new List<string>(10);
         {
            var ending = false;
            while (!ending)
            {
               var language = languages[random.Next(languages.Count)];
               if (word.Length > approximateDesiredLength - 3)
                  ending = true;
               word += language.GetPart(word, ending, partsUsed);
            }
         }
         return word;
      }

      public void LoadWords(string[] words)
      {
         foreach (var partDictionary in _parts)
            partDictionary.Clear();
         _nextLetterFrequency.Clear();
         _secondLetterFrequency.Clear();
         _wordBeginnings.Clear();
         _wordEndings.Clear();
         _firstLetterFrequency.Clear();
         _lastLetterFrequency.Clear();
         _totalWords = 0;
         foreach (var line in words)
         {
            var word = line;
            if (word.Contains(' '))
               word = word.Split(' ')[0];
            if (word.Length < 2)
               continue;

            _totalWords++;

            for (var i = 2; i <= Math.Min(4, word.Length); i++)
            {
               var beginning = word[..i];
               if (!_wordBeginnings.TryAdd(beginning, 1))
                  _wordBeginnings[beginning]++;

               var ending = word.Substring(word.Length - i, i);
               if (!_wordEndings.TryAdd(ending, 1))
                  _wordEndings[ending]++;
            }

            for (var letters = 2; letters < 5; letters++)
            {
               var partsDictionary = _parts[letters - 2];

               for (var i = 0; i <= word.Length - letters; i++)
               {
                  var substring = word.Substring(i, letters);
                  if (substring.Length == 2)
                     if (substring[0] == substring[1])
                        continue;

                  if (!partsDictionary.TryAdd(substring, 1))
                     partsDictionary[substring]++;
               }
            }

            for (var i = 0; i < word.Length - 1; i++)
            {
               var letter = word.Substring(i, 1);
               var nextLetter = word.Substring(i + 1, 1);
               _nextLetterFrequency.TryAdd(letter, new());
               if (!_nextLetterFrequency[letter].TryAdd(nextLetter, 1))
                  _nextLetterFrequency[letter][nextLetter]++;
            }

            for (var i = 0; i < word.Length - 2; i++)
            {
               var letter = word.Substring(i, 1);
               var nextLetter = word.Substring(i + 2, 1);
               _secondLetterFrequency.TryAdd(letter, new());
               if (!_secondLetterFrequency[letter].TryAdd(nextLetter, 1))
                  _secondLetterFrequency[letter][nextLetter]++;
            }

            for (var i = 0; i < word.Length - 3; i++)
            {
               var letter = word.Substring(i, 1);
               var nextLetter = word.Substring(i + 3, 1);
               _thirdLetterFrequency.TryAdd(letter, new());
               if (!_thirdLetterFrequency[letter].TryAdd(nextLetter, 1))
                  _thirdLetterFrequency[letter][nextLetter]++;
            }

            var firstLetter = word[..1];
            if (!_firstLetterFrequency.TryAdd(firstLetter, 1))
               _firstLetterFrequency[firstLetter]++;

            var lastLetter = word.Substring(word.Length - 1, 1);
            if (!_lastLetterFrequency.TryAdd(lastLetter, 1))
               _lastLetterFrequency[lastLetter]++;
         }

         _unusualLetterThreshold = _totalWords / 500;
      }

      public void LoadSyllables(string[] syllables)
      {
         _usesSimpleSyllables = true;
         foreach (var line in syllables)
         {
            var syllable = line;
            if (syllable.Contains(' '))
               syllable = syllable.Split(' ')[0];
            if (syllable.Length == 0)
               continue;

            _simpleSyllables.Add(syllable);
         }
      }

      public override string ToString() => Name;
   }
}