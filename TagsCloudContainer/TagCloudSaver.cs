using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ResultOf;

namespace TagsCloudContainer
{
    public class TagCloudSaver
    {
        private readonly Dictionary<string, IBitmapSaver> saversByExtension;

        public TagCloudSaver(IEnumerable<IBitmapSaver> bitmapSavers)
        {
            saversByExtension = bitmapSavers
                .SelectMany(bs => bs.SupportedExtensions.Select(ext => (ext, bs)))
                .ToDictionary();
        }

        public Result<None> Save(Color bgColor, List<WordLayout> wordLayouts, string path)
        {
            var bitmap = TagCloudDrawer.DrawTagCloud(wordLayouts, bgColor);

            return FindBitmapSaver(Path.GetExtension(path)).Then(saver => saver.Save(bitmap, path));
        }

        private Result<IBitmapSaver> FindBitmapSaver(string extension)
        {
            if (extension == null)
            {
                return Result.Fail<IBitmapSaver>(nameof(extension) + " is null");
            }

            if (!saversByExtension.TryGetValue(extension, out var findBitmapSaver))
            {
                return Result.Fail<IBitmapSaver>($"Unsupported extension: \"{extension}\"");
            }

            return Result.Ok(findBitmapSaver);
        }
    }
}
