using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using Fisco.Utility.Constants;
using SkiaSharp;

namespace Fisco.Component
{
    /// <summary>
    /// Componente para representação de textos com suporte para SkiaSharp
    /// </summary>
    public class Text : IFiscoComponent, IDisposable, IDrawable
    {
        /// <summary>
        /// Cor do pincel
        /// </summary>
        public SKColor Brush { get; private set; }
        /// <summary>
        /// Fonte do texto
        /// </summary>
        public SKFont TextFont { get; private set; }
        /// <summary>
        /// Conteúdo de texto
        /// </summary>
        public string TextContent { get; private set; }

        private readonly ItemAlign _align;

        /// <summary>
        /// Cria um novo elemento de texto
        /// </summary>
        public Text(SKFont font, string text, ItemAlign align, SKColor brush)
        {
            TextFont = font ?? throw new ArgumentNullException(nameof(font));
            TextContent = text ?? string.Empty;
            _align = align;
            Brush = brush;
        }

        private string[] GetLines()
        {
            if (string.IsNullOrEmpty(TextContent))
                return [];

            return TextContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        }

        private SKSize MeasureString()
        {
            if (string.IsNullOrEmpty(TextContent) || TextFont == null)
                return SKSize.Empty;

            using var paint = new SKPaint { Typeface = TextFont.Typeface, TextSize = TextFont.Size };
            var metrics = paint.FontMetrics;
            float lineSpacing = metrics.Descent - metrics.Ascent + metrics.Leading;
            if (lineSpacing <= 0)
                lineSpacing = TextFont.Size * 1.2f;

            var lines = GetLines();
            if (lines.Length == 0)
                return SKSize.Empty;

            float maxWidth = 0;
            foreach (var line in lines)
            {
                float w = paint.MeasureText(line);
                if (w > maxWidth)
                    maxWidth = w;
            }

            float textAscentDescent = metrics.Descent - metrics.Ascent;
            float totalHeight = lines.Length <= 1
                ? textAscentDescent
                : ((lines.Length - 1) * lineSpacing + textAscentDescent);

            return new SKSize(maxWidth, totalHeight);
        }

        void IDrawable.Draw(ref SKCanvas g, ref Context drawContext)
        {
            var size = MeasureString();
            if (!drawContext.IgnoreOutBoundsError)
            {
                if (size.Width > drawContext.Width)
                    throw new OutOfBoundsException(FiscoConstants.NO_COMPONENT_FITS);
            }

            var lines = GetLines();
            if (lines.Length == 0)
                return;

            using var paint = new SKPaint
            {
                Typeface = TextFont.Typeface,
                TextSize = TextFont.Size,
                Color = Brush,
                IsAntialias = true
            };

            var metrics = paint.FontMetrics;
            float fontAscent = Math.Abs(metrics.Ascent);
            float lineSpacing = metrics.Descent - metrics.Ascent + metrics.Leading;
            if (lineSpacing <= 0)
                lineSpacing = TextFont.Size * 1.2f;

            float startY = drawContext.TopOffSet + drawContext.GetStartHeight;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                float lineWidth = paint.MeasureText(line);

                float lineX = _align switch
                {
                    ItemAlign.Left => drawContext.LeftOffSet,
                    ItemAlign.Center => drawContext.LeftOffSet + (drawContext.Width - drawContext.LeftOffSet - lineWidth) / 2f,
                    ItemAlign.Right => drawContext.Width - lineWidth,
                    _ => drawContext.LeftOffSet
                };

                float baselineY = startY + (i * lineSpacing) + fontAscent;
                g.DrawText(line, lineX, baselineY, paint);
            }

            drawContext.UpdateHeight((int)Math.Ceiling(size.Height));
        }

        void IDrawable.DrawInsideTable(ref SKCanvas g, SKRect region)
        {
            if (string.IsNullOrEmpty(TextContent))
                return;

            using var paint = new SKPaint
            {
                Typeface = TextFont.Typeface,
                TextSize = TextFont.Size,
                Color = Brush,
                IsAntialias = true
            };

            var metrics = paint.FontMetrics;
            float fontAscent = Math.Abs(metrics.Ascent);
            float fontDescent = metrics.Descent;
            float textHeight = fontAscent + fontDescent;

            var lines = GetLines();
            float lineSpacing = metrics.Descent - metrics.Ascent + metrics.Leading;
            if (lineSpacing <= 0)
                lineSpacing = TextFont.Size * 1.2f;

            float totalTextHeight = lines.Length <= 1 ? textHeight : ((lines.Length - 1) * lineSpacing + textHeight);
            float startY = region.Top + Math.Max(0, (region.Height - totalTextHeight) / 2f);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                float lineWidth = paint.MeasureText(line);

                float lineX = _align switch
                {
                    ItemAlign.Left => region.Left + 2,
                    ItemAlign.Center => region.Left + Math.Max(0, (region.Width - lineWidth) / 2f),
                    ItemAlign.Right => region.Right - lineWidth - 2,
                    _ => region.Left + 2
                };

                float baselineY = startY + (i * lineSpacing) + fontAscent;
                g.DrawText(line, lineX, baselineY, paint);
            }
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            TextFont?.Dispose();
        }
    }
}
