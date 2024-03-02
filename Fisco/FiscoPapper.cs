using Fisco.Component;
using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace Fisco
{
    public class FiscoPapper : IDisposable
    {
        private Graphics _g;
        private Bitmap _img;
        private Context _context;
        private readonly List<IFiscoComponent> components = new List<IFiscoComponent>();

        private void InitGraphics()
        {
            _img = GraphicsGenerator.GenerateBitmapField(_context);
            _g = GraphicsGenerator.GenerateGraphicsObject(ref _img, Color.White);

            _context.Width = _img.Width;
            _context.Height = _img.Height;
        }

        public FiscoPapper(BobineSize size, int leftOffset, int topOffset, bool igonreOutOfBounds)
        {
            _context = new Context(size, igonreOutOfBounds)
            {
                LeftOffSet = leftOffset,
                TopOffSet = topOffset,
            };

            InitGraphics();
        }

        public FiscoPapper(BobineSize size, bool igonreOutOfBounds)
        {
            _context = new Context(size, igonreOutOfBounds);
            InitGraphics();
        }

        public void AddComponent(IFiscoComponent component)
        {
            components.Add(component);
        }

        public Bitmap Render()
        {
            _g.Clear(Color.White);
            foreach (IDrawable component in components.Cast<IDrawable>())
            {
                component.Draw(ref _g, ref _context);
            }

            return _img;
        }

        public void Print()
        {
            try
            {
                int[] sizes = BobineProps.GetSizes(_context.BobineSize);
                PaperSize papel = new PaperSize("Custom Size", sizes[0], sizes[1]);
                PrintDocument doc = new PrintDocument();

                doc.DefaultPageSettings.PaperSize = papel;
                doc.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawImage(Render(), new PointF(0, 0));
                };


                doc.Print();
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("O objeto de renderização era inválido");
            }
            catch (InvalidPrinterException innerException)
            {
                throw innerException;
            }
        }

        public void Dispose()
        {
            _g.Dispose();
            foreach (IFiscoComponent component in components)
                component?.Dispose();

            components.Clear();
            _img.Dispose();
        }
    }
}
