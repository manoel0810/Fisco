using Fisco.Enumerator;
using System;

namespace Fisco.Utility
{
    /// <summary>
    /// Utilitários para propriedades e dimensões de bobinas térmicas
    /// </summary>
    public static class BobineProps
    {
        /// <summary>
        /// Obtém as dimensões em milímetros (largura x altura) para o tipo de bobina especificado
        /// </summary>
        /// <param name="size">Tipo de bobina térmica</param>
        /// <returns>Array contendo [largura, altura] em mm</returns>
        public static int[] GetSizes(BobineSize size)
        {
            return size switch
            {
                BobineSize._58x297mm => [58, 297],
                BobineSize._58x3276mm => [58, 3276],
                BobineSize._80x297mm => [80, 297],
                BobineSize._80x3276mm => [80, 3276],
                _ => ParseFromEnumName(size)
            };
        }

        private static int[] ParseFromEnumName(BobineSize size)
        {
            string name = size.ToString().Replace("mm", "").Replace("_", "");
            string[] parts = name.Split('x');

            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                return [w, h];
            }

            return [80, 297];
        }

        /// <summary>
        /// Converte as dimensões da bobina (em mm) para pixels usando o fator PPI padrão (legado .NET Framework)
        /// </summary>
        /// <param name="size">Tipo de bobina térmica</param>
        /// <returns>Array contendo [largura, altura] em pixels</returns>
        public static float[] GetSizesUsingPPI(BobineSize size)
        {
            const decimal legacyFactor = 3.405m;
            return GetSizesUsingPPIFactor(size, legacyFactor);
        }

        /// <summary>
        /// Converte as dimensões da bobina (em mm) para pixels com base no DPI especificado
        /// </summary>
        /// <param name="size">Tipo de bobina térmica</param>
        /// <param name="dpi">DPI (pontos por polegada) da impressora/resolução</param>
        /// <returns>Array contendo [largura, altura] em pixels</returns>
        public static float[] GetSizesUsingPPI(BobineSize size, int dpi)
        {
            if (dpi <= 0)
                throw new ArgumentException("DPI deve ser maior que 0.", nameof(dpi));

            var mmSizes = GetSizes(size);
            float widthInPixels = (float)(mmSizes[0] / 25.4 * dpi);
            float heightInPixels = (float)(mmSizes[1] / 25.4 * dpi);

            return [widthInPixels, heightInPixels];
        }

        /// <summary>
        /// Converte as dimensões da bobina para pixels usando fator legado para .NET Framework
        /// </summary>
        /// <param name="size">Tipo de bobina térmica</param>
        /// <param name="ppiFactor">Fator de escala legado</param>
        /// <returns>Array contendo [largura, altura] em pixels</returns>
        public static float[] GetSizesUsingPPIFactor(BobineSize size, decimal ppiFactor)
        {
            var mmSizes = GetSizes(size);
            return [(float)(mmSizes[0] * (double)ppiFactor), (float)(mmSizes[1] * (double)ppiFactor)];
        }
    }
}
