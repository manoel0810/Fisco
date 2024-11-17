using Fisco.Component;
using Fisco.Exceptions;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;

namespace Fisco.Utility
{
    internal class GraphicsGenerator
    {
        // Constante para conversão de mm para polegadas
        private const decimal MM_TO_INCH = 25.4m;

        public static SKCanvas GenerateGraphicsObject(ref SKBitmap img, SKColor backColor)
        {
            SKCanvas canva = new(img);
            canva.Clear(backColor);
            return canva;
        }

        public static SKBitmap GenerateBitmapField(Context context, int dpi)
        {
            if (dpi <= 0)
                throw new ArgumentException("DPI deve ser maior que 0.", nameof(dpi));

            // Dimensões do papel em mm
            float[] sizes = BobineProps.GetSizesUsingPPI(context.BobineSize, dpi);
            float widthInMm = sizes[0];
            float heightInMm = sizes[1];

            // Conversão de mm para pixels
            int widthInPixels = (int)(widthInMm / (float)MM_TO_INCH * dpi);
            int heightInPixels = (int)(heightInMm / (float)MM_TO_INCH * dpi);

            // Criar bitmap com as dimensões calculadas
            SKImage papper = SKImage.Create(new SKImageInfo((int)widthInMm, (int)heightInMm));
            SKBitmap map = SKBitmap.FromImage(papper);

            return map;
        }

        public static SKBitmap ImageTrim(SKBitmap img, SKPoint xoy, Context context)
        {
            Validate(img, xoy, context);

            try
            {
                var trimRect = new SKRectI(
                    (int)xoy.X,
                    (int)xoy.Y,
                    (int)(xoy.X + context.Width),
                    (int)(xoy.Y + context.GetStartHeight + (GraphicsGeneratorConstants.SECURITY_MARGIN * 3))
                );

                using (var trimmedImage = new SKBitmap(trimRect.Width, trimRect.Height))
                {
                    using (var canvas = new SKCanvas(trimmedImage))
                    {
                        canvas.DrawBitmap(img, trimRect, new SKRect(0, 0, trimRect.Width, trimRect.Height));
                    }

                    return trimmedImage.Copy();
                }
            }
            catch (OutOfMemoryException)
            {
                return null!;
            }
        }

        private static void Validate(SKBitmap img, SKPoint xoy, Context context)
        {
            if (img == null || context == null)
                throw new ArgumentNullException((img == null ? nameof(img) : nameof(context)));

            if (xoy.X < 0 || xoy.X > img.Width)
                throw new ArgumentOutOfRangeException(nameof(xoy), xoy, GraphicsGeneratorConstants.oX_OUT_RANGE);

            if (xoy.Y < 0 || xoy.Y > img.Height)
                throw new ArgumentOutOfRangeException(nameof(xoy), xoy, GraphicsGeneratorConstants.oY_OUT_RANGE);

            if (img.Width != context.Width || img.Height != context.Height)
                throw new FiscoException(GraphicsGeneratorConstants.SIZES_NO_MATCH);
        }
    }
}
