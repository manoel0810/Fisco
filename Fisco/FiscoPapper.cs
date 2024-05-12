using Fisco.Component;
using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Utility;
using Fisco.Utility.Constants.Specific;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace Fisco
{
    /// <summary>
    /// Permite criar um documento e adicionar componentes do tipo <see cref="IFiscoComponent"/> ao mesmo
    /// </summary>

    public class FiscoPapper : IDisposable
    {
        private Graphics _g;
        private Bitmap _img;
        private Bitmap _renderedImage;
        private Context _context;
        private bool _rendered = false;
        private bool _disposed = false;
        private readonly List<IFiscoComponent> components = new List<IFiscoComponent>();

        private void InitGraphics()
        {
            _img = GraphicsGenerator.GenerateBitmapField(_context);
            _g = GraphicsGenerator.GenerateGraphicsObject(ref _img, Color.White);

            _context.Width = _img.Width;
            _context.Height = _img.Height;
        }

        /// <summary>
        /// Cria uma nova instância do <see cref="FiscoPapper"/>
        /// </summary>
        /// <param name="size">Define o tamanho da bobina térmica</param>
        /// <param name="leftOffset">Margem de recuo esquerdo</param>
        /// <param name="topOffset">Margem de recuo superior</param>
        /// <param name="igonreOutOfBounds">Quando true, ignora áreas fora dos limites de desenho</param>

        public FiscoPapper(BobineSize size, int leftOffset, int topOffset, bool igonreOutOfBounds)
        {
            _context = new Context(size, igonreOutOfBounds)
            {
                LeftOffSet = leftOffset,
                TopOffSet = topOffset,
            };

            InitGraphics();
        }

        /// <summary>
        /// Cria uma nova instância do <see cref="FiscoPapper"/>
        /// </summary>
        /// <param name="size">Define o tamanho da bobina térmica</param>
        /// <param name="igonreOutOfBounds">Quando true, ignora áreas fora dos limites de desenho</param>

        public FiscoPapper(BobineSize size, bool igonreOutOfBounds)
        {
            _context = new Context(size, igonreOutOfBounds);
            InitGraphics();
        }

        /// <summary>
        /// Adicionar um novo <see cref="IFiscoComponent"/> no contexto atual do documento
        /// </summary>
        /// <param name="component">Componente que será adicionado</param>

        public void AddComponent(IFiscoComponent component)
        {
            components.Add(component);
        }

        /// <summary>
        /// Inicia a renderização do documento, devolvendo um <see cref="Bitmap"/> com a imagem final
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>

        public Bitmap Render()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_rendered)
                return _img;

            _g.Clear(Color.White);
            foreach (IDrawable component in components.Cast<IDrawable>())
            {
                component.Draw(ref _g, ref _context);
            }

            _renderedImage = _img;
            _rendered = _renderedImage != null;

            var cut = GraphicsGenerator.ImageTrim(_img, new Point(), _context);
            _img = cut ?? _img;

            return _img;
        }

        /// <summary>
        /// Tenta imprimir o documento usando a impressora padrão definida
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentException"></exception>

        public void Print()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                int[] sizes = BobineProps.GetSizes(_context.BobineSize);
                PaperSize papel = new PaperSize("Custom Size", sizes[0], sizes[1]);
                PrintDocument doc = new PrintDocument();

                doc.DefaultPageSettings.PaperSize = papel;
                doc.PrintPage += (sender, e) =>
                {
                    var image = _renderedImage ?? Render();
                    if (image != null)
                        e.Graphics.DrawImage(image, new PointF(0, 0));
                    else
                        throw new Exception();
                };


                doc.Print();
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(FiscoPapperConstants.INVALID_RENDER_OBJECT);
            }
            catch (InvalidPrinterException innerException)
            {
                throw innerException;
            }
        }

        void IDisposable.Dispose()
        {
            _g?.Dispose();
            foreach (IFiscoComponent component in components)
                component?.Dispose();

            components.Clear();
            _img?.Dispose();

            _disposed = true;
        }
    }
}
