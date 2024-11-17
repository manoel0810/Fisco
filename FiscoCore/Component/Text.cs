using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;
using System.Drawing;

namespace Fisco.Component
{

    /// <summary>
    /// Componente para representação de textos
    /// </summary>
    /// <remarks>
    /// Cria um novo elemento gráfico do tipo <see cref="IFiscoComponent"/> para renderização com suporte para textos
    /// </remarks>
    /// <param name="font">Fonte do texto</param>
    /// <param name="text">Conteúdo</param>
    /// <param name="align">Alinhamento</param>
    /// <param name="brush">Pincel</param>

    public class Text(SKFont font, string text, ItemAlign align, SKColor brush) : IFiscoComponent, IDisposable, IDrawable
    {
        /// <summary>
        /// Cor do pincel
        /// </summary>
        public SKColor Brush { get; private set; } = brush;
        /// <summary>
        /// Fonte do texto
        /// </summary>
        public SKFont TextFont { get; private set; } = font;
        /// <summary>
        /// Conteúdo de texto
        /// </summary>
        public string TextContent { get; private set; } = text;

        private readonly ItemAlign _align = align;

        private SKSize MeasureString()
        {
            if (string.IsNullOrEmpty(TextContent) || TextFont == null)
                return SKSize.Empty;

            using (var paint = new SKPaint { Typeface = TextFont.Typeface, TextSize = TextFont.Size })
            {
                return new SKSize(paint.MeasureText(TextContent), paint.FontMetrics.CapHeight + GraphicsGeneratorConstants.SECURITY_MARGIN);
            }
        }

        static PointF GetCoordenate(Rectangle objectSize, Context drawContext, ItemAlign itemAlign)
        {
            if (itemAlign == ItemAlign.Left)
            {
                return new Point(drawContext.LeftOffSet, drawContext.TopOffSet + drawContext.GetStartHeight);
            }
            else if (itemAlign == ItemAlign.Center)
            {
                //ignore left margin
                int startPoint = (drawContext.GetSizes()[0] - objectSize.Width) / 2;
                return new Point(startPoint, drawContext.TopOffSet + drawContext.GetStartHeight);
            }
            else if (itemAlign == ItemAlign.Right)
            {
                //ignore left margin
                int leftMargin = drawContext.GetSizes()[0] - objectSize.Width;
                return new Point(leftMargin, drawContext.TopOffSet + drawContext.GetStartHeight);
            }

            throw new NoDeterministicsException(FiscoConstants.NO_ALIGN_PASSED);
        }

        private float CalculateTopOffset(float percent)
        {
            return MeasureString().Height * (percent / 100);
        }

        private static float GetPercentage(float fontSize)
        {
            return (30 * fontSize) / 22;
        }

        private SKPoint GetTableCoordenate(ref SKCanvas g, SKRect region)
        {
            int margin = 2;
            float y = region.Top + (MeasureString().Height / 2) + CalculateTopOffset(GetPercentage(TextFont.Size));

            // Obtém a largura do texto usando SKPaint
            var textPaint = new SKPaint
            {
                Typeface = TextFont.Typeface,
                TextSize = TextFont.Size
            };

            float textWidth = GetTextWidth(TextContent, textPaint);

            return _align switch
            {
                ItemAlign.Left => new SKPoint(region.Left + margin, y),
                ItemAlign.Center => new SKPoint(region.Left + ((region.Width - textWidth) / 2), y),
                ItemAlign.Right => new SKPoint(region.Right - textWidth, y),
                _ => throw new NoDeterministicsException(FiscoConstants.INVALID_ALIGN),
            };
        }

        private static float GetTextWidth(string text, SKPaint paint)
        {
            return paint.MeasureText(text);
        }

        private Rectangle GetObjectRectangle(SKCanvas g)
        {
            var size = MeasureString();
            return new Rectangle(0, 0, (int)size.Width, (int)size.Height);
        }

        void IDrawable.Draw(ref SKCanvas g, ref Context drawContext)
        {
            if (!drawContext.IgnoreOutBoundsError)
            {
                if (MeasureString().Width > drawContext.GetSizes()[0])
                    throw new OutOfBoundsException(FiscoConstants.NO_COMPONENT_FITS);
            }

            Rectangle r = GetObjectRectangle(g);
            using (var paint = new SKPaint { Typeface = TextFont.Typeface, TextSize = TextFont.Size, Color = Brush })
            {
                var coordenate = GetCoordenate(r, drawContext, _align);
                g.DrawText(TextContent, coordenate.X, coordenate.Y, paint);
            }

            drawContext.UpdateHeight(r.Height);
        }

        void IDrawable.DrawInsideTable(ref SKCanvas g, SKRect region)
        {
            using (var paint = new SKPaint
            {
                Typeface = TextFont.Typeface,
                TextSize = TextFont.Size,
                Color = Brush
            })
            {
                var coordenate = GetTableCoordenate(ref g, region);
                g.DrawText(TextContent, coordenate.X, coordenate.Y, paint);
            }
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            TextFont.Dispose();
        }
    }
}
