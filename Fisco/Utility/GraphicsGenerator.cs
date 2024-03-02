using Fisco.Component;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Fisco.Utility
{
    internal class GraphicsGenerator
    {
        //const to 96 PPI
        public const decimal PPI_FACTOR = 3.779527559055118m;
        public const int SCALE = 1;

        public static Graphics GenerateGraphicsObject(ref Bitmap img, Color backColor)
        {
            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.Bicubic;
            g.ScaleTransform(SCALE, SCALE);

            g.Clear(backColor);
            return g;
        }

        public static Bitmap GenerateBitmapField(Context context)
        {
            float[] sizes = BobineProps.GetSizesUsingPPI(context.BobineSize);
            float width = sizes[0] * SCALE;
            float height = sizes[1] * SCALE;

            Bitmap papper = new Bitmap((int)width, (int)height);
            return papper;
        }
    }
}
