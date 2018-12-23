using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ResultOf;

namespace TagsCloudContainer
{
    public class BitmapSaver : IBitmapSaver
    {
        private readonly Dictionary<string, ImageFormat> formats = new Dictionary<string, ImageFormat>
        {
            [".bmp"] = ImageFormat.Bmp,
            [".emf"] = ImageFormat.Emf,
            [".exif"] = ImageFormat.Exif,
            [".gif"] = ImageFormat.Gif,
            [".ico"] = ImageFormat.Icon,
            [".jpeg"] = ImageFormat.Jpeg,
            [".jpg"] = ImageFormat.Jpeg,
            [".png"] = ImageFormat.Png,
            [".tiff"] = ImageFormat.Tiff,
            [".wmf"] = ImageFormat.Wmf
        };

        public Result<None> Save(Bitmap bitmap, string filename)
        {
            var extension = Path.GetExtension(filename);
            if (extension == null)
            {
                return Result.Fail<None>("No extension found.");
            }

            if (!formats.TryGetValue(extension.ToLowerInvariant(), out var imageFormat))
            {
                return Result.Fail<None>("Unsupported extension: " + extension);
            }

            bitmap.Save(filename, imageFormat);

            return Result.Ok();
        }

        public string[] SupportedExtensions => formats.Select(p => p.Key).ToArray();
    }
}
