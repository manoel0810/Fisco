using Fisco.Component;
using Fisco.Exceptions;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;

namespace Fisco.Utility
{
    internal class GraphicsGenerator
    {
        //const to 96 PPI
        public const decimal PPI_FACTOR = 3.405m; //3.779527559055118m;
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

        [SecurityCritical]
        public static Bitmap ImageTrim(Bitmap img, Point xoy, Context context)
        {
            Validade(img, xoy, context);

            unsafe
            {
                try
                {
                    var trim = img.Clone(new Rectangle(xoy, new Size(context.Width, context.GetStartHeight + GraphicsGeneratorConstants.SECURITY_MARGING)), img.PixelFormat);
                    return trim;
                }
                catch (OutOfMemoryException)
                {
                    return null;
                }
            }
        }

        private static void Validade(Bitmap img, Point xoy, Context context)
        {
            if (img == null || context == null)
                throw new ArgumentNullException(FiscoConstants.NULL_ARGUMENT);

            if (xoy.X < 0 || xoy.X > img.Width)
                throw new ArgumentOutOfRangeException(GraphicsGeneratorConstants.oX_OUT_RANGE);

            if (xoy.Y < 0 || xoy.Y > img.Height)
                throw new ArgumentOutOfRangeException(GraphicsGeneratorConstants.oY_OUT_RANGE);

            if (img.Width != context.Width || img.Height != context.Height)
                throw new FiscoException(GraphicsGeneratorConstants.SIZES_NO_MATCH);

        }
    }
}
