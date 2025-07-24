using System.Text;

namespace Arcanum.Core.CoreSystems.ParsingSystem;

// Define a clear enum for the parsing state to replace the 'wasEquals' byte.
file enum ParsingState : byte
{
   Default, // Looking for a key or a block name
   SawEquals, // Just saw an '=', the next non-quoted token is a value
   InValue // Currently processing a value (either quoted or unquoted)
}

public static class ElementParser
{
   public static unsafe (List<Block>, List<Content>) GetElements(string path, string input)
   {
      var contents = new List<Content>();
      var blocks = new List<Block>();
      var currentContent = new StringBuilder();
      var blockStack = new ModifiableStack<Block>();

      var isInQuotes = false;
      var isInWord = false;
      var isInWhiteSpace = false;
      var contentStart = 0;
      var elementIndex = 0;

      var state = ParsingState.Default;

      var prevWordStart = -1;
      var prevWordEnd = -1;

      var remainingInput = input.AsSpan();
      var lineIndex = 0;

      while (!remainingInput.IsEmpty)
      {
         var lineBreakIndex = remainingInput.IndexOf('\n');
         ReadOnlySpan<char> line;
         if (lineBreakIndex == -1)
         {
            line = remainingInput;
            remainingInput = ReadOnlySpan<char>.Empty;
         }
         else
         {
            line = remainingInput.Slice(0, lineBreakIndex);
            remainingInput = remainingInput.Slice(lineBreakIndex + 1);
         }

         if (!line.IsEmpty && line[^1] == '\r')
         {
            line = line.Slice(0, line.Length - 1);
         }

         // Per-line state is reset here, which is correct.
         var length = line.Length;
         var charIndex = 0;
         var wordStart = -1;
         var wordEnd = -1;
         isInWord = false;
         isInWhiteSpace = false;

         if (line.Length == 0)
         {
            currentContent.Append('\n');
            lineIndex++;
            continue;
         }

         while (charIndex < length)
         {
            var c = line[charIndex];
            switch (c)
            {
               case '\\':
                  currentContent.Append(c);
                  if (isInQuotes)
                  {
                     charIndex++;
                     if (line.Length > charIndex)
                        currentContent.Append(line[charIndex]);
                  }

                  break;

               case '"':
                  // If we see a quote after an equals sign, we are now parsing a value.
                  if (state == ParsingState.SawEquals)
                     state = ParsingState.InValue;
                  isInQuotes = !isInQuotes;
                  isInWhiteSpace = false;
                  currentContent.Append(c);
                  break;

               case '{':
                  if (isInQuotes)
                  {
                     currentContent.Append(c);
                     break;
                  }

                  if (currentContent.Length < 1)
                  {
                     Console.WriteLine($"Error in file {path}: Block name cannot be empty at line {lineIndex + 1}, char {charIndex + 1}");
                     return ([], []);
                  }

                  // This is the "heinous formatting" check that must be preserved.
                  if (wordEnd < 0 || wordStart < 0)
                  {
                     if (prevWordStart < 0 || prevWordEnd < 0)
                     {
                        Console.WriteLine($"Error in file {path}: Block name cannot be empty at line {lineIndex + 1}, char {charIndex + 1}");
                        return ([], []);
                     }

                     wordStart = prevWordStart;
                     wordEnd = prevWordEnd;
                     Console.WriteLine($"Warning in file {path}: Block name is empty at line {lineIndex + 1}, char {charIndex + 1}. Using previous block name.");
                  }

                  prevWordStart = -1;
                  prevWordEnd = -1;

                  var nameLength = wordEnd - wordStart;
                  Span<char> charSpan = stackalloc char[nameLength];
                  currentContent.CopyTo(wordStart, charSpan, nameLength);
                  currentContent.Remove(wordStart, currentContent.Length - wordStart);
                  Block newBlock;
                  wordStart = -1;
                  wordEnd = -1;

                  var (trimmedStart, trimmedLength) = GetTrimmedRange(currentContent);
                  if (trimmedLength > 0)
                  {
                     var contentValue = currentContent.ToString(trimmedStart, trimmedLength);
                     var content = new Content(contentValue, contentStart, elementIndex++);
                     newBlock = new(new(charSpan), lineIndex, elementIndex++);
                     if (blockStack.IsEmpty)
                     {
                        contents.Add(content);
                        blocks.Add(newBlock);
                     }
                     else
                     {
                        blockStack.Peek().ContentElements.Add(content);
                        blockStack.Peek().SubBlocks.Add(newBlock);
                     }
                  }
                  else
                  {
                     newBlock = new(new(charSpan), lineIndex, elementIndex++);
                     if (blockStack.IsEmpty)
                        blocks.Add(newBlock);
                     else
                        blockStack.Peek().SubBlocks.Add(newBlock);
                  }

                  currentContent.Clear();
                  blockStack.Push(newBlock);
                  contentStart = lineIndex;
                  state = ParsingState.Default; 
                  break;

               case '}':
                  if (isInQuotes)
                  {
                     currentContent.Append(c);
                     break;
                  }

                  if (blockStack.IsEmpty)
                  {
                     Console.WriteLine($"Error in file {path}: Unmatched closing brace at line {lineIndex + 1}, char {charIndex + 1}");
                     return ([], []);
                  }

                  var (closeTrimmedStart, closeTrimmedLength) = GetTrimmedRange(currentContent);
                  if (closeTrimmedLength > 0)
                  {
                     var closingContentValue = currentContent.ToString(closeTrimmedStart, closeTrimmedLength);
                     var closingContentStartLine = (currentContent.Length > 0 && currentContent[0] == '\n')
                                                      ? contentStart + 1
                                                      : contentStart;
                     var content = new Content(closingContentValue, closingContentStartLine, elementIndex++);
                     blockStack.Peek().ContentElements.Add(content);
                     wordStart = -1;
                     wordEnd = -1;
                     currentContent.Clear();
                  }

                  blockStack.Pop();
                  contentStart = lineIndex;
                  state = ParsingState.Default; 
                  break;

               case '#':
                  if (!isInQuotes)
                  {
                     charIndex = length;
                     break;
                  }

                  currentContent.Append(c);
                  break;

               case '\r':
                  if (charIndex != length - 1)
                  {
                     Console.WriteLine($"Error in file {path}: Unexpected carriage return at line {lineIndex + 1}, char {charIndex + 1}");
                     if (currentContent.Length >= 1 &&
                         char.IsWhiteSpace(currentContent[^1]) &&
                         currentContent[^1] != '\n')
                        currentContent.Remove(currentContent.Length - 1, 1);
                     currentContent.Append('\n');
                     state = ParsingState.Default; 
                  }

                  break;

               default:
                  if (!isInQuotes)
                  {
                     if (char.IsWhiteSpace(c))
                     {
                        if (!isInWhiteSpace)
                        {
                           isInWhiteSpace = true;
                           isInWord = false;
                           if (wordStart != -1)
                              // This logic is for multi-value pairs like `list = { a b c }`
                              // The original code used `wasEquals > 2`, this is the direct equivalent.
                              if (state == ParsingState.InValue)
                              {
                                 state = ParsingState.Default;
                                 currentContent.Append('\t');
                              }
                              else
                              {
                                 currentContent.Append(' ');
                              }
                        }

                        break;
                     }

                     isInWhiteSpace = false;
                     if (c != '=')
                     {
                        if (!isInWord)
                        {
                           wordEnd = currentContent.Length + 1;
                           wordStart = currentContent.Length;
                           isInWord = true;
                           // If we see a word after an equals sign, it's a value.
                           if (state == ParsingState.SawEquals)
                              state = ParsingState.InValue;
                        }
                        else
                        {
                           wordEnd = currentContent.Length + 1;
                        }
                     }
                     else // c is '='
                     {
                        isInWord = false;
                        state = ParsingState.SawEquals;
                     }
                  }

                  currentContent.Append(c);
                  break;
            }

            charIndex++;
         }

         if (currentContent.Length >= 1 && char.IsWhiteSpace(currentContent[^1]) && currentContent[^1] != '\n')
            currentContent.Remove(currentContent.Length - 1, 1);

         if (wordStart >= 0 && wordEnd >= 0)
         {
            prevWordStart = wordStart;
            prevWordEnd = wordEnd;
         }

         currentContent.Append('\n');
         state = ParsingState.Default; 
         lineIndex++;
      }

      if (!blockStack.IsEmpty)
      {
         Console.WriteLine($"Error in file {path}: Unmatched opening brace at line {blockStack.Peek().StartLine + 1}");
         return ([], []);
      }

      var (finalTrimmedStart, finalTrimmedLength) = GetTrimmedRange(currentContent);
      if (finalTrimmedLength > 0)
      {
         contents.Add(new(currentContent.ToString(finalTrimmedStart, finalTrimmedLength),
                          contentStart,
                          elementIndex++));
      }

      return (blocks, contents);
   }

   private static (int start, int length) GetTrimmedRange(StringBuilder sb)
   {
      var start = 0;
      while (start < sb.Length && char.IsWhiteSpace(sb[start]))
      {
         start++;
      }

      var end = sb.Length - 1;
      while (end > start && char.IsWhiteSpace(sb[end]))
      {
         end--;
      }

      if (end < start)
         return (0, 0);

      return (start, end - start + 1);
   }
}