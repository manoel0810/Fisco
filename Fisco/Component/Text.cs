using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using System;
using System.Drawing;

namespace Fisco.Component
{
    public class Text : IFiscoComponent, IDisposable, IDrawable
    {
        public Brush Brush { get; private set; } = Brushes.Black;
        public Font TextFont { get; private set; }
        public string TextContent { get; private set; }

        private readonly ItemAlign _align;

        public Text(Font font, string text, ItemAlign align, Brush brush)
        {
            TextFont = font;
            TextContent = text;
            _align = align;

            this.Brush = brush;
        }

        private SizeF MensureString(Graphics g, string text, Font font)
        {
            return g.MeasureString(text, font);
        }

        PointF GetCoordenate(Rectangle objectSize, Context drawContext, ItemAlign itemAlign)
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

            throw new NoDeterministicsException("Nenhum ItemAlign válido foi passado");
        }

        private PointF GetTableCoordenate(ref Graphics g, Rectangle region)
        {
            int marging = 0;
            float Y = region.Y;
            switch (_align)
            {
                case ItemAlign.Left:
                    return new PointF(region.X + marging, Y);
                case ItemAlign.Center:
                    return new PointF(region.X + ((region.Width - GetObjectRectangle(g, TextContent, TextFont).Width) / 2), Y);
                case ItemAlign.Right:
                    return new PointF(region.X + (region.Width - GetObjectRectangle(g, TextContent, TextFont).Width), Y);
                default:
                    throw new NoDeterministicsException("O alinhamento informado era inválido");
            }
        }

        private Rectangle GetObjectRectangle(Graphics g, string text, Font font)
        {
            SizeF size = g.MeasureString(text, font);
            return new Rectangle(0, 0, (int)size.Width, (int)size.Height);
        }

        void IDrawable.Draw(ref Graphics g, ref Context drawContext)
        {
            if (!drawContext.IgnoreOutBoundsError)
            {
                if (MensureString(g, TextContent, TextFont).Width > drawContext.GetSizes()[0])
                    throw new OutOfBoundsException("O comprimento para impressão é insuficiente para o componente");
            }

            Rectangle r = GetObjectRectangle(g, TextContent, TextFont);
            g.DrawString(TextContent, TextFont, Brush, GetCoordenate(r, drawContext, _align));
            drawContext.UpdateHeight(r.Height);
        }

        void IDrawable.DrawInsideTable(ref Graphics g, Rectangle region)
        {
            g.DrawString(TextContent, TextFont, Brush, GetTableCoordenate(ref g, region));
        }

        public void Dispose()
        {
            TextFont.Dispose();
        }
    }
}
