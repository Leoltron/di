using System.Drawing;
using ResultOf;

namespace TagsCloudContainer
{
    public interface IBitmapSaver
    {
        Result<None> Save(Bitmap bitmap, string filename);

        string[] SupportedExtensions { get; }
    }
}
