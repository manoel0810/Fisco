using Fisco.Enumerator;

namespace Fisco.Component
{
    public class Context
    {
        private int _actualHeight = 0;

        public BobineSize BobineSize { get; private set; }
        public bool IgnoreOutBoundsError { get; private set; }
        public int GetStartHeight => _actualHeight;
        public int LeftOffSet { get; set; }
        public int TopOffSet { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }


        public void UpdateHeight(int height) => _actualHeight += height;

        public Context(BobineSize size, bool ignoreOutBoundsError)
        {
            BobineSize = size;
            IgnoreOutBoundsError = ignoreOutBoundsError;
        }

        public int[] GetSizes()
        {
            return new int[] { Width, Height };
        }
    }
}
