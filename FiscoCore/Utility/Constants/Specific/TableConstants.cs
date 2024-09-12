namespace Fisco.Utility.Constants.Specific
{
    internal static class TableConstants
    {
        public const int SECURITY_MARGIN = 0;
        public const int MAX_WIDTH_PERCENTAGE = 100;
        public const int MIN_TABLE_COLUMNS_COUNT = 1;
        public const string VALUES_OF_COLUNMS_NO_MATCH = "A quantidade de valores passada é diferente do número de colunas";
        public const string SUM_PERCENTAGE_MAX_MESSAGE = "A soma dos valores para tamanho das colunas não deve ser superior a 100%";
        public const string SUM_PERCENTAGE_MIN_MESSAGE = "A soma dos valores para tamanho das colunas não deve ser inferior a 100%";
        public const string INCONSISTENTE_ROW_MATCH_MESSAGE = "Linha inconsistente com o modelo de tabela atual";
        public const string MAX_COLUMN_ITENS_EXCEDED_MESSAGE = "Não é possível adicionar mais uma coluna à tabela\nLimite máximo de arg0 colunas";
        public const string MAX_CELL_ITENS_EXCEDED_MESSAGE = "Não é possível adicionar mais uma célula à linha\nLimite máximo de arg0 células";
        public const string NOT_SUPORTED_EXCEPTION_MESSAGE = "Atualmente, não é suportado o recurso de renderização de tabelas dentro de tabelas";
        public const string MIN_TABLE_COLUMN_COUNT_MESSAGE = "O número de colunas da tabela deve ser superior a 1";
    }
}
