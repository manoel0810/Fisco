using System;

namespace Fisco.Enumerator
{
    [Flags]
    public enum ItemAlign : uint
    {
        None = 0,
        Left = 1,
        Center = 2,
        Right = 3,
    }

    [Flags]
    public enum BobineSize : uint
    {
        _58x297mm,
        _58x3276mm,
        _80x297mm,
        _80x3276mm,
    }
}
