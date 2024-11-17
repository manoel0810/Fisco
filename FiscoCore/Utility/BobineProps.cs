using Fisco.Enumerator;

namespace Fisco.Utility
{
    internal class BobineProps
    {
        public static int[] GetSizes(BobineSize size)
        {
            //_58x297mm
            string _name_ = $"{size}";
            _name_ = _name_.Replace("mm", "").Replace("_", "");
            string[] _part_ = _name_.Split('x');

            int w = int.Parse(_part_[0]);
            int h = int.Parse(_part_[1]);

            return [w, h];
        }

        public static float[] GetSizesUsingPPI(BobineSize size, int dpi)
        {
            if (dpi <= 0)
                throw new ArgumentException("DPI deve ser maior que 0.", nameof(dpi));

            // Obter tamanhos da bobina (em milímetros)
            var currentSize = GetSizes(size);

            // Converter de milímetros para pixels usando DPI
            float widthInPixels = (float)(currentSize[0] / 25.4 * dpi); // 25.4 mm por polegada
            float heightInPixels = (float)(currentSize[1] / 25.4 * dpi);

            return new float[] { widthInPixels, heightInPixels };
        }
    }
}
