using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using System;
using System.Drawing;

namespace Fisco.Component
{
    /// <summary>
    /// Componente para representação de imagens
    /// </summary>
    public class Image : IFiscoComponent, IDisposable, ICloneable, IDrawable
    {
        private readonly Bitmap _bmp;
        private readonly ItemAlign _align;

        /// <summary>
        /// Obtém ou define o <see cref="Context"/> de trabalho
        /// </summary>
        public Context FiscoContext { get; private set; }
        /// <summary>
        /// Obtém as dimensões da imagem atual
        /// </summary>
        /// <returns></returns>
        public Size GetDim() => _bmp.Size;

        /// <summary>
        /// Cria um novo elemento gráfico do tipo <see cref="IFiscoComponent"/> para renderização com suporte para imagens
        /// </summary>
        /// <param name="image">Imagem</param>
        /// <param name="align">Alinhamento</param>
        /// <exception cref="ArgumentNullException"></exception>

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
                    throw new OutOfBoundsException(ImageConstants.IMAGE_NO_FITS);
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

            throw new NoDeterministicsException(FiscoConstants.NO_ALIGN_PASSED);
        }

        void IDrawable.Draw(ref Graphics g, ref Context drawContext)
        {
            CheckFits(drawContext);
            g.DrawImage(_bmp, GetCoordenate());
            drawContext.UpdateHeight(_bmp.Height);
        }

        void IDrawable.DrawInsideTable(ref Graphics g, Rectangle region)
        {
            if (region.Width < _bmp.Width || region.Height < _bmp.Height)
                throw new OutOfBoundsException(ImageConstants.OUT_OF_BOUNDS_MESSAGE);

            g.DrawImage(_bmp, new Point(region.X, region.Y));
        }

        void IDisposable.Dispose()
        {
            _bmp.Dispose();
        }

        /// <summary>
        /// Clona o objeto atual
        /// </summary>
        /// <returns></returns>

        public object Clone()
        {
            return new Image(_bmp, _align)
            {
                FiscoContext = FiscoContext
            };
        }
    }
}
