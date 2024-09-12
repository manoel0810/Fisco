using Fisco.Enumerator;
using Fisco.Exceptions;

namespace Fisco.Component
{
    /// <summary>
    /// Contexto no qual o documento se encontra
    /// </summary>

    public class Context
    {
        private int _actualHeight = 0;

        /// <summary>
        /// Tipo de bobina
        /// </summary>
        public BobineSize BobineSize { get; private set; }

        /// <summary>
        /// Não disparar erros do tipo <see cref="OutOfBoundsException"/> caso o componente seja maior que a área disponível
        /// </summary>
        public bool IgnoreOutBoundsError { get; private set; }
        /// <summary>
        /// Devolve o valor Y para começar um desenho no contexto
        /// </summary>
        public int GetStartHeight => _actualHeight;
        /// <summary>
        /// Devolve o recuo esquedo definido no construtor do objeto <see cref="FiscoPapper"/> na instância atual
        /// </summary>
        public int LeftOffSet { get; set; }
        /// <summary>
        /// Devolve o recuo superior definido no construtor do objeto <see cref="FiscoPapper"/> na instância atual
        /// </summary>
        public int TopOffSet { get; set; }

        /// <summary>
        /// Largura disponível para desenho
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Comprimento disponível para desenho
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Atualiza a coordenada Y do contexto, com base em elementos já renderizados
        /// </summary>
        /// <param name="height">Acrescimo do valor Y</param>
        public void UpdateHeight(int height) => _actualHeight += height;


        /// <summary>
        /// Cria uma nova instância para o objeto do tipo <see cref="Context"/>
        /// </summary>
        /// <param name="size">Define o tipo de bobina</param>
        /// <param name="ignoreOutBoundsError">Quando true, ignora áreas fora dos limites de desenho</param>
        public Context(BobineSize size, bool ignoreOutBoundsError)
        {
            BobineSize = size;
            IgnoreOutBoundsError = ignoreOutBoundsError;
        }

        /// <summary>
        /// Devolve o comprimento e largura da região de trabalho
        /// </summary>
        /// <returns></returns>
        public int[] GetSizes()
        {
            return [Width, Height];
        }
    }
}
