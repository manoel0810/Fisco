using Fisco.Component;
using Fisco.Exceptions;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;

namespace Fisco.Utility
{
    internal class GraphicsGenerator
    {
        //const to 96 PPI
        public const decimal PPI_FACTOR = 3.405m; //3.779527559055118m;
        public const int SCALE = 1;

        public static SKCanvas GenerateGraphicsObject(ref SKBitmap img, SKColor backColor)
        {
            SKCanvas canva = new(img);
            canva.Clear(backColor);

            return canva;
        }

        public static SKBitmap GenerateBitmapField(Context context)
        {
            float[] sizes = BobineProps.GetSizesUsingPPI(context.BobineSize);
            float width = sizes[0] * SCALE;
            float height = sizes[1] * SCALE;

            SKImage papper = SKImage.Create(new SKImageInfo((int)width, (int)height));
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
                    (int)(xoy.Y + context.GetStartHeight + GraphicsGeneratorConstants.SECURITY_MARGIN)
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
