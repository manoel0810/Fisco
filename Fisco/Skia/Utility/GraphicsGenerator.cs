using Fisco.Component;
using Fisco.Exceptions;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;

namespace Fisco.Utility
{
    internal static class GraphicsGenerator
    {
        public static SKCanvas GenerateGraphicsObject(ref SKBitmap img, SKColor backColor)
        {
            SKCanvas canvas = new(img);
            canvas.Clear(backColor);
            return canvas;
        }

        public static SKBitmap GenerateBitmapField(Context context, int dpi)
        {
            if (dpi <= 0)
                throw new ArgumentException("DPI deve ser maior que 0.", nameof(dpi));

            float[] pixelSizes = BobineProps.GetSizesUsingPPI(context.BobineSize, dpi);
            int width = (int)Math.Round(pixelSizes[0]);
            int height = (int)Math.Round(pixelSizes[1]);

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var map = new SKBitmap(info);

            using (var canvas = new SKCanvas(map))
            {
                canvas.Clear(SKColors.White);
            }

            return map;
        }

        public static SKBitmap ImageTrim(SKBitmap img, SKPoint xoy, Context context)
        {
            Validate(img, xoy, context);

            try
            {
                int contentHeight = context.TopOffSet + context.GetStartHeight + GraphicsGeneratorConstants.SECURITY_MARGIN;
                int targetHeight = Math.Min(contentHeight, img.Height);
                int targetWidth = Math.Min(context.Width, img.Width);

                if (targetHeight <= 0 || targetWidth <= 0)
                    return img;

                int startX = Math.Clamp((int)xoy.X, 0, img.Width - 1);
                int startY = Math.Clamp((int)xoy.Y, 0, img.Height - 1);
                int rectWidth = Math.Min(targetWidth, img.Width - startX);
                int rectHeight = Math.Min(targetHeight, img.Height - startY);

                var trimRect = new SKRectI(startX, startY, startX + rectWidth, startY + rectHeight);
                var trimmedImage = new SKBitmap(rectWidth, rectHeight);

                using (var canvas = new SKCanvas(trimmedImage))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawBitmap(img, trimRect, new SKRect(0, 0, rectWidth, rectHeight));
                }

                return trimmedImage;
            }
            catch (OutOfMemoryException)
            {
                return null!;
            }
        }

        private static void Validate(SKBitmap img, SKPoint xoy, Context context)
        {
            if (img == null)
                throw new ArgumentNullException(nameof(img));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (xoy.X < 0 || xoy.X > img.Width)
                throw new ArgumentOutOfRangeException(nameof(xoy), xoy, GraphicsGeneratorConstants.oX_OUT_RANGE);

            if (xoy.Y < 0 || xoy.Y > img.Height)
                throw new ArgumentOutOfRangeException(nameof(xoy), xoy, GraphicsGeneratorConstants.oY_OUT_RANGE);

            if (img.Width != context.Width || img.Height != context.Height)
                throw new FiscoException(GraphicsGeneratorConstants.SIZES_NO_MATCH);
        }
    }
}
