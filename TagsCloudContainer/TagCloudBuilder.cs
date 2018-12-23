using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ResultOf;
using TagsCloudVisualization;

namespace TagsCloudContainer
{
    public class TagCloudBuilder
    {
        private readonly WordPreprocessor wordPreprocessor;
        private readonly ILayouter layouter;
        private readonly IWordColorPicker wordColorPicker;
        private readonly IWordFontPicker wordFontPicker;
        private readonly Dictionary<string, IFileWordsProvider> wordProviderByExtension;

        public TagCloudBuilder(
            ILayouter layouter,
            IEnumerable<IFileWordsProvider> wordProviders,
            IWordColorPicker wordColorPicker,
            IWordFontPicker wordFontPicker,
            WordPreprocessor wordPreprocessor)
        {
            this.layouter = layouter;
            this.wordColorPicker = wordColorPicker;
            this.wordFontPicker = wordFontPicker;
            this.wordPreprocessor = wordPreprocessor;

            wordProviderByExtension = wordProviders
                .SelectMany(wp => wp.AcceptedExtensions.Select(ext => (ext, wp)))
                .ToDictionary();
        }

        public Result<IEnumerable<WordLayout>> Build(string filePath, Color wordColor, Color bgColor, float fontSize)
        {
            wordColorPicker.SetBackgroundColor(bgColor);
            wordColorPicker.SetBaseWordColor(wordColor);
            wordFontPicker.SetBaseSize(fontSize);

            return GetWords(filePath)
                .Then(words => wordPreprocessor.PreprocessWords(words))
                .Then(CountWords)
                .Then(Build);
        }

        private IEnumerable<WordLayout> Build(List<(string word, int count)> wordCount)
        {
            var wordColors = wordColorPicker.PickColors(wordCount);
            var wordFonts = wordFontPicker.PickFonts(wordCount);

            foreach (var tuple in wordCount.OrderByDescending(t => t.count))
            {
                var word = tuple.word;
                var font = wordFonts[word];
                var color = wordColors[word];
                var rectangle = layouter.PutNextRectangle(font.GetStringSize(word).ToSize());

                yield return new WordLayout(word, rectangle, font, color);
            }
        }

        private static List<(string word, int count)> CountWords(IEnumerable<string> words)
        {
            var wordCount = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (!wordCount.ContainsKey(word))
                    wordCount[word] = 0;
                wordCount[word]++;
            }

            return wordCount.Select(pair => (pair.Key, pair.Value)).ToList();
        }

        private Result<string[]> GetWords(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == null)
            {
                return Result.Fail<string[]>("Extension is null");
            }

            if (!wordProviderByExtension.TryGetValue(extension, out var wordsProvider))
            {
                return Result.Fail<string[]>("Unsupported extension: \"{extension}\"");
            }

            return Result.Of(() => File.OpenRead(filePath))
                .Then(s => (s, wordsProvider.GetWords(s)))
                .Then(pair =>
                {
                    var (stream, words) = pair;
                    stream.Close();
                    return words;
                });
        }
    }
}
