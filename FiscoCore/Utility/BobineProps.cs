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

        public static float[] GetSizesUsingPPI(BobineSize size)
        {
            var currentSize = GetSizes(size);
            return [(float)(currentSize[0] * GraphicsGenerator.PPI_FACTOR), (float)(currentSize[1] * GraphicsGenerator.PPI_FACTOR)];
        }
    }
}
