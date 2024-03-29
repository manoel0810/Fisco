using System;

namespace Fisco.Enumerator
{
    /// <summary>
    /// Alinhamento dos elementos gráficos
    /// </summary>
    [Flags]
    public enum ItemAlign : uint
    {
        /// <summary>
        /// Indeterminado
        /// </summary>
        None = 0,
        /// <summary>
        /// Alinhar à esquerda
        /// </summary>
        Left = 1,
        /// <summary>
        /// Alinhar no centro
        /// </summary>
        Center = 2,
        /// <summary>
        /// Alinhar à direita
        /// </summary>
        Right = 3,
    }

    /// <summary>
    /// Tipos de bobinas suportadas
    /// </summary>
    [Flags]
    public enum BobineSize : uint
    {
        /// <summary>
        /// 58mm X 297mm
        /// </summary>
        _58x297mm,
        /// <summary>
        /// 58mm X 3276mm
        /// </summary>
        _58x3276mm,
        /// <summary>
        /// 80mm X 297mm
        /// </summary>
        _80x297mm,
        /// <summary>
        /// 80mm X 3276mm
        /// </summary>
        _80x3276mm,
    }
}
