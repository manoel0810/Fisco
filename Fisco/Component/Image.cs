using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using System;
using System.Drawing;

namespace Fisco.Component
{
    public class Image : IFiscoComponent, IDisposable, ICloneable, IDrawable
    {
        private readonly Bitmap _bmp;
        private readonly ItemAlign _align;
        public Context FiscoContext { get; private set; }
        public Size GetDim() => _bmp.Size;

        public Image(Bitmap image, ItemAlign align)
        {
            _bmp = image;
            _align = align;

            if (image == null)
                throw new ArgumentNullException(nameof(image));
        }

        bool NoFits()
        {
            return (_bmp.Width > FiscoContext.Width) || (_bmp.Height > (FiscoContext.Height - FiscoContext.GetStartHeight));
        }

        private void CheckFits(Context context)
        {
            FiscoContext = context;

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.IgnoreOutBoundsError)
                if (NoFits())
                    throw new OutOfBoundsException("a imagem não cabe na área disponível");
        }

        PointF GetCoordenate()
        {
            if (_align == ItemAlign.Left)
                return new PointF(FiscoContext.LeftOffSet, FiscoContext.GetStartHeight + FiscoContext.TopOffSet);
            else if (_align == ItemAlign.Center)
            {
                int startPoint = (FiscoContext.Width - _bmp.Width) / 2;
                return new PointF(startPoint, FiscoContext.GetStartHeight + FiscoContext.TopOffSet);
            }
            else if (_align == ItemAlign.Right)
            {
                int leftMargin = FiscoContext.Width - _bmp.Width;
                return new PointF(leftMargin, FiscoContext.GetStartHeight + FiscoContext.TopOffSet);
            }

            throw new NoDeterministicsException("Nenhum ItemAlign válido foi passado");
        }

        public void Draw(ref Graphics g, ref Context drawContext)
        {
            CheckFits(drawContext);
            g.DrawImage(_bmp, GetCoordenate());
            drawContext.UpdateHeight(_bmp.Height);
        }

        public void DrawInsideTable(ref Graphics g, Rectangle region)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _bmp.Dispose();
        }

        public object Clone()
        {
            return new Image(_bmp, _align)
            {
                FiscoContext = FiscoContext
            };
        }       
    }
}
