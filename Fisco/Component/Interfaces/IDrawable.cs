using System.Drawing;

namespace Fisco.Component.Interfaces
{
    interface IDrawable
    {
        void Draw(ref Graphics g, ref Context drawContext);
        void DrawInsideTable(ref Graphics g, Rectangle region);
    }
}
