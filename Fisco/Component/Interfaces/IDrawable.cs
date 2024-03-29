﻿using System.Drawing;

namespace Fisco.Component.Interfaces
{
    /// <summary>
    /// Componente passivo à renderização 
    /// </summary>

    interface IDrawable
    {
        /// <summary>
        /// Desenha no contexto do <see cref="FiscoPapper"/>, usando de um <see cref="Graphics"/> com base no contexto <see cref="Context"/>
        /// </summary>
        /// <param name="g">Referência ao <see cref="Graphics"/> principal do documento</param>
        /// <param name="drawContext">Referência ao <see cref="Context"/> principal do documento</param>
        void Draw(ref Graphics g, ref Context drawContext);

        /// <summary>
        /// Desenha no contexto do <see cref="FiscoPapper"/>, usando de um <see cref="Graphics"/> com base no contexto <see cref="Context"/>, em um sub contexto de tabelas
        /// </summary>
        /// <param name="g">Referência ao <see cref="Graphics"/> principal do documento</param>
        /// <param name="region">Referência ao <see cref="Context"/> principal do documento</param>
        void DrawInsideTable(ref Graphics g, Rectangle region);
    }
}
