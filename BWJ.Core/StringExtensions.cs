using System.Collections.Generic;
using System.Linq;

namespace BWJ.Core
{
    public static class StringExtensions
    {
        public static string ToKebabCase(this string name)
        {
            var words = ToWordArray(name).Select(w => w.ToLower());
            return string.Join('-', words);
        }

        public static string ToCamelCase(this string name)
        {
            var words = ToWordArray(name);
            words[0] = words[0].ToLower();
            for(var i = 1; i < words.Length; i++)
            {
                words[i] = CapitalizeWord(words[i].ToLower());
            }

            return string.Join(string.Empty, words);
        }

        public static string ToPascalCase(this string name)
        {
            var words = ToWordArray(name).Select(w => CapitalizeWord(w.ToLower()));

            return string.Join(string.Empty, words);
        }

        public static string ToTitleText(this string name)
        {
            var words = ToWordArray(name).Select(w => CapitalizeWord(w));

            return string.Join(" ", words);
        }

        public static string ToSentence(this string name)
        {
            var words = ToWordArray(name);
            words[0] = CapitalizeWord(words[0]);
            for (var i = 1; i < words.Length; i++)
            {
                words[i] = SentenceCaseWord(words[i]);
            }

            return string.Join(string.Empty, words);
        }

        public static string[] ToWords(this string name)
            => ToWordArray(name);

        private static string CapitalizeWord(string word)
        {
            if(string.IsNullOrWhiteSpace(word) || IsAcronym(word)) { return word; }

            word = word.ToLower();
            var firstChar = word[0];

            return $"{char.ToUpper(firstChar)}{word.Substring(1)}";
        }

        private static string SentenceCaseWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || IsAcronym(word)) { return word; }

            return word.ToLower();
        }

        private static bool IsAcronym(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) { return false; }

            int capitalCount = 0;
            foreach (var @char in word)
            {
                if (char.IsUpper(@char)) { capitalCount++; }
            }

            return word.Length == capitalCount;
        }

        private static string[] ToWordArray(string text)
        {
            var words = new List<string>();
            var currentWord = string.Empty;

            for (var idx = 0; idx < text.Length; idx++)
            {
                char? prevLetter = idx == 0 ? null : text[idx - 1];
                var currentLetter = text[idx];
                char? nextLetter = (idx + 1) == text.Length ? null : text[idx + 1];
                if(PreviousCharacterNotAlphanumeric(prevLetter)
                    || IsUppercaseCharacterForNewWord(currentLetter, prevLetter, nextLetter)
                    || IsDigitForNewWord(currentLetter, prevLetter))
                {
                    if (string.IsNullOrEmpty(currentWord) == false)
                    {
                        words.Add(currentWord);
                        currentWord = string.Empty;
                    }
                }

                if(char.IsLetterOrDigit(currentLetter))
                {
                    currentWord += currentLetter;
                }
            }

            if (string.IsNullOrEmpty(currentWord) == false)
            {
                words.Add(currentWord);
            }

            return words.ToArray();
        }

        private static bool PreviousCharacterNotAlphanumeric(char? prevChar)
            => prevChar == null || char.IsLetterOrDigit(prevChar ?? '-') == false;
        private static bool IsUppercaseCharacterForNewWord(char currentChar, char? prevChar, char? nextChar)
            => char.IsUpper(currentChar) &&
            (char.IsLower(prevChar ?? '-') || char.IsDigit(prevChar ?? '-') || char.IsLower(nextChar ?? '-'));
        private static bool IsDigitForNewWord(char currentChar, char? prevChar)
            => char.IsDigit(currentChar) && char.IsDigit(prevChar ?? '-') == false;
    }
}
