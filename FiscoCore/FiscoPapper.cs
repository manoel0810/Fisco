﻿using Fisco.Component;
using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Utility;
using SkiaSharp;

namespace Fisco
{
    /// <summary>
    /// Permite criar um documento e adicionar componentes do tipo <see cref="IFiscoComponent"/> ao mesmo
    /// </summary>

    public class FiscoPapper : IDisposable
    {
        private SKCanvas? canvas;
        private SKBitmap? _img;
        private SKImage? _renderedImage;
        private Context _context;
        private bool _rendered = false;
        private bool _disposed = false;
        private readonly List<IFiscoComponent> components = [];

        private const string BACK_COLOR = "#ffffff";

        private void InitGraphics(int dpi)
        {
            _img = GraphicsGenerator.GenerateBitmapField(_context, dpi);
            canvas = GraphicsGenerator.GenerateGraphicsObject(ref _img, SKColor.Parse(BACK_COLOR));

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
        /// <param name="dpi">DPI da impressora térmica</param>

        public FiscoPapper(BobineSize size, int leftOffset, int topOffset, bool igonreOutOfBounds, int dpi = 128)
        {
            if (dpi == 0)
                throw new Exception("O DPI deve ser superior a zero");

            _context = new Context(size, igonreOutOfBounds, dpi)
            {
                LeftOffSet = leftOffset,
                TopOffSet = topOffset,
            };

            InitGraphics(dpi);
        }

        /// <summary>
        /// Cria uma nova instância do <see cref="FiscoPapper"/>
        /// </summary>
        /// <param name="size">Define o tamanho da bobina térmica</param>
        /// <param name="igonreOutOfBounds">Quando true, ignora áreas fora dos limites de desenho</param>
        /// <param name="dpi">DPI da impressora térmica</param>

        public FiscoPapper(BobineSize size, bool igonreOutOfBounds, int dpi = 128)
        {
            if (dpi == 0)
                throw new Exception("O DPI deve ser superior a zero");

            _context = new Context(size, igonreOutOfBounds, dpi);
            InitGraphics(dpi);
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
        /// Inicia a renderização do documento, devolvendo um <see cref="SKImage"/> com a imagem final
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>

        public SKImage Render()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_rendered)
                return _renderedImage!;

            canvas!.Clear(SKColor.Parse(BACK_COLOR));
            foreach (IDrawable component in components.Cast<IDrawable>())
            {
                component.Draw(ref canvas, ref _context);
            }

            canvas.Flush();
            _renderedImage = SKImage.FromBitmap(_img);
            _rendered = _renderedImage != null;

            var cut = GraphicsGenerator.ImageTrim(_img!, new SKPoint(0, 0), _context);
            _img = cut ?? _img;

            using (var data = _img!.Encode(SKEncodedImageFormat.Png, 100))
            {
                if (data == null)
                {
                    throw new Exception("Falha ao codificar a imagem.");
                }

                _renderedImage = SKImage.FromEncodedData(data);
            }

            return _renderedImage!;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);

            canvas?.Dispose();
            foreach (IFiscoComponent component in components)
                component?.Dispose();

            components.Clear();
            _img?.Dispose();

            _disposed = true;
        }
    }
}
