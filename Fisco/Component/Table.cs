using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions.Table.Cells;
using Fisco.Exceptions.Table.Columns;
using Fisco.Exceptions.Table.Rows;
using Fisco.Utility;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fisco.Component
{
    /// <summary>
    /// Componente para representação de tabelas
    /// </summary>

    public class Table : IFiscoComponent, IDrawable
    {
        private Bitmap _tableBitmap;
        private Graphics _tableGraphics;
        private readonly BobineSize _size;
        private int _tableRealHeight = 0;

        /// <summary>
        /// Define a quebra automatica de linhas do cabeçalho
        /// </summary>
        public bool RowWrap { get; set; } = false;
        /// <summary>
        /// Define o número de colunas da tabela
        /// </summary>
        public int ColumnCount { get; private set; }
        /// <summary>
        /// Obtém a porcentagem de cada coluna com relção a largura disponível
        /// </summary>
        public float[] UsePercentage { get; private set; }
        /// <summary>
        /// Define o pincel para desenhar o cabeçalho
        /// </summary>
        public Pen TableLineColor { get; set; } = new Pen(Brushes.Black, 1.5f);

        private readonly bool _ignoreOutBoundsError;
        /// <summary>
        /// Colunas da tabela
        /// </summary>
        public readonly Column Columns;
        /// <summary>
        /// Linhas da tabela
        /// </summary>
        public readonly Row Rows;

        //---------------------------------------------------------------------------------//

        private int _currentXPosition = 0;
        private int _currentYPosition = 0;

        private Point GetCurrentPosition() => new Point(_currentXPosition, _currentYPosition);

        //private Point GetNewPointFromVector(Point unit) => new Point(_currentXPosition - unit.X, _currentYPosition - unit.Y);

        //---------------------------------------------------------------------------------//


        /// <summary>
        /// Cria um novo elemento gráfico do tipo <see cref="IFiscoComponent"/> para renderização com suporte para tabelas
        /// </summary>
        /// <param name="columnsCount">Número total de colunas</param>
        /// <param name="size">Tipo da bobina </param>
        /// <param name="ignoreOutBoundsError">Quando true, ignora áreas fora dos limites de desenho</param>

        public Table(int columnsCount, BobineSize size, bool ignoreOutBoundsError = false)
        {
            ColumnCount = columnsCount;
            _size = size;
            _ignoreOutBoundsError = ignoreOutBoundsError;

            Columns = new Column(ColumnCount);
            Rows = new Row(Columns);

            LoadWidths();
        }

        private void LoadWidths()
        {
            float partValue = (float)TableConstants.MAX_WIDTH_PERCENTAGE / ColumnCount;
            float[] values = new float[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
                values[i] = partValue;

            SetPercentage(values);
        }

        /// <summary>
        /// Retorna uma nova <see cref="TableRow"/> baseado no esquema tual da tabela
        /// </summary>
        /// <returns></returns>

        public TableRow GetNewRow()
        {
            return new TableRow(ColumnCount);
        }

        /// <summary>
        /// Define a porcentagem de cada coluna com relção a largura disponível
        /// </summary>
        /// <param name="widths">Medidas</param>
        /// <exception cref="InvalidWidthsColumnException"></exception>

        public void SetPercentage(float[] widths)
        {
            if (widths.Length != ColumnCount)
                throw new InvalidWidthsColumnException(TableConstants.VALUES_OF_COLUNMS_NO_MATCH);

            if ((int)widths.Sum() > TableConstants.MAX_WIDTH_PERCENTAGE)
                throw new InvalidWidthsColumnException(TableConstants.SUM_PERCENTAGE_MAX_MESSAGE);

            if ((int)widths.Sum() < TableConstants.MAX_WIDTH_PERCENTAGE)
                throw new InvalidWidthsColumnException(TableConstants.SUM_PERCENTAGE_MIN_MESSAGE);

            UsePercentage = widths;
        }

        void IDisposable.Dispose()
        {
            _tableGraphics.Dispose();
            _tableBitmap.Dispose();
        }

        private void InitBitmap()
        {
            _tableBitmap = GraphicsGenerator.GenerateBitmapField(new Context(_size, _ignoreOutBoundsError));
            _tableGraphics = GraphicsGenerator.GenerateGraphicsObject(ref _tableBitmap, Color.White);
        }

        private SizeF EstimateCharSizeOnBitmap(string text, Font f)
        {
            return _tableGraphics.MeasureString(text, f);
        }

        private void UpdateXPosition(Rectangle rectangle)
        {
            _currentXPosition += rectangle.Width;
        }

        /*
        private void UpdateYPosition(Rectangle rectangle)
        {
            _currentYPosition += rectangle.Height;
        }
        */

        private void NextRow(Rectangle rectangle)
        {
            _currentXPosition = 0;
            _currentYPosition += rectangle.Height;
        }

        private void DrawFrame(Rectangle region, TableCell ui = null)
        {
            PointF[] points =
            {
                new PointF(region.X, region.Y),
                new PointF(region.X, region.Y + region.Height),
                new PointF(region.X + region.Width, region.Y + region.Height),
                new PointF(region.X + region.Width, region.Y),
                new PointF(region.X, region.Y),
            };

            if (ui != null)
                _tableGraphics.FillRectangle(ui.GetBrush(), region);

            _tableGraphics.DrawLines(TableLineColor, points);
        }

        private void DrawRegion(Rectangle region, Brush backColor)
        {
            _tableGraphics.FillRectangle(backColor, region);
        }

        private float GetRealWidth(bool incluseSecurityMargin = true)
        {
            return BobineProps.GetSizesUsingPPI(_size)[0] - (incluseSecurityMargin ? TableConstants.SECURITY_MARGIN : 0);
        }

        private float GetRealSizeByPercentage(float percentage)
        {
            return (GetRealWidth() * percentage) / (float)TableConstants.MAX_WIDTH_PERCENTAGE;
        }

        private void DrawComponent(IDrawable component, Rectangle region)
        {
            component.DrawInsideTable(ref _tableGraphics, region);
        }

        private void DrawHeader()
        {
            if (Columns.GetColumns().Count != ColumnCount)
                return;

            int i = 0;
            Font drawFont = Columns.HeaderFont;
            int[] avaibleColumnSizes = new int[ColumnCount];
            string[] headersText = new string[ColumnCount];
            int maxHeight = 0;

            foreach (var column in Columns.GetColumns())
            {
                string text = column.ColumnDisplayName;
                var avaibleSize = GetRealSizeByPercentage(UsePercentage[i]);

                if (RowWrap)
                {
                    var size = EstimateCharSizeOnBitmap(text, drawFont);
                    if (size.Width > avaibleSize)
                    {
                        var unitValue = size.Width / text.Length;
                        var charPerLine = ((int)avaibleSize / (int)unitValue) - 1;

                        for (int j = charPerLine; j < text.Length; j += charPerLine + 1)
                        {
                            text = text.Insert(j, "\n");
                        }
                    }
                }

                var txtSize = EstimateCharSizeOnBitmap(text, drawFont);
                headersText[i] = text;

                if (txtSize.Height > maxHeight)
                    maxHeight = (int)txtSize.Height;

                avaibleColumnSizes[i] = (int)avaibleSize;
                i++;
            }

            i = 0;
            _tableRealHeight += maxHeight;
            foreach (var column in Columns.GetColumns())
            {
                var rec = new Rectangle(GetCurrentPosition(), new Size(avaibleColumnSizes[i], maxHeight));
                if (column.DrawBackColor)
                    DrawRegion(rec, Columns.BackColor);

                DrawFrame(rec);
                UpdateXPosition(rec);

                Text t = new Text(drawFont, headersText[i], ItemAlign.Center, Columns.ForeGroundColor);
                DrawComponent(t, rec);

                i++;
                if (i >= UsePercentage.Length)
                {
                    NextRow(rec);
                    i = 0;
                }
            }
        }

        private Rectangle[] CreateGridLineRegion(int maxHeight)
        {
            int i = Columns.GetColumns().Count;
            Rectangle[] regions = new Rectangle[i];
            for (int j = 0; j < i; j++)
            {
                regions[j] = new Rectangle(GetCurrentPosition(), new Size((int)GetRealSizeByPercentage(UsePercentage[j]), maxHeight));
                UpdateXPosition(regions[j]);
            }

            NextRow(regions[0]);
            return regions;
        }

        private void DrawTableGrid()
        {
            InitBitmap();
            DrawHeader();

            foreach (TableRow row in Rows.GetRows())
            {
                var cells = row.GetCells();
                int rowHeight = 0;
                for (int i = 0; i < cells.Count; i++)
                {
                    TableCell cell = cells[i];
                    if (cell.Component is Image img)
                    {
                        if (img.GetDim().Height > rowHeight)
                        {
                            rowHeight = img.GetDim().Height;
                        }
                    }
                    else if (cell.Component is Text text)
                    {
                        float h = _tableGraphics.MeasureString(text.TextContent, text.TextFont).Height;
                        if (h > rowHeight)
                            rowHeight = (int)h;
                    }
                }

                var regions = CreateGridLineRegion(rowHeight);
                int e = 0;

                foreach (Rectangle rec in regions)
                {
                    var tableElement = cells[e++];
                    var uiElement = (IDrawable)(tableElement.Component);

                    DrawFrame(rec, tableElement);
                    DrawComponent(uiElement, rec);
                }

                _tableRealHeight += rowHeight;
            }
        }

        /*
        private Point GetStartPont()
        {
            var realSize = GetRealWidth(false);
            var sizeWithMargin = GetRealWidth();

            var startX = (realSize - sizeWithMargin) / 2;
            return new Point((int)startX - 15, _currentYPosition);
        }
        */

        void IDrawable.Draw(ref Graphics g, ref Context drawContext)
        {
            DrawTableGrid();
            g.DrawImage(_tableBitmap, new Point(0, drawContext.GetStartHeight + drawContext.TopOffSet));
            drawContext.UpdateHeight(_tableRealHeight + drawContext.TopOffSet);
        }

        void IDrawable.DrawInsideTable(ref Graphics g, Rectangle region)
        {
            throw new NotSupportedException(TableConstants.NOT_SUPORTED_EXCEPTION_MESSAGE);
        }

        /// <summary>
        /// Representa a coleção de linhas de um <see cref="Table"/>
        /// </summary>

        public class Row
        {
            private readonly Column _model;
            private readonly List<TableRow> _rows = new List<TableRow>();

            /// <summary>
            /// Cria uma nova <see cref="Row"/> com base no modelo de colunas
            /// </summary>
            /// <param name="model">Modelo de colunas</param>

            public Row(Column model)
            {
                _model = model;
            }

            /// <summary>
            /// Obtém uma coleção com todas as linhas
            /// </summary>
            /// <returns></returns>

            public ICollection<TableRow> GetRows()
            {
                return _rows;
            }

            /// <summary>
            /// Adiciona uma nova linha ao esquema de linhas da tabela atual
            /// </summary>
            /// <param name="row">Nova linha</param>
            /// <exception cref="RowException"></exception>

            public void Add(TableRow row)
            {
                if (row.GetCells().Count > _model.GetColumns().Count || row.GetCells().Count <= 0)
                    throw new RowException(TableConstants.INCONSISTENTE_ROW_MATCH_MESSAGE);

                _rows.Add(row);
            }

            /// <summary>
            /// Remove uma linha do esquema de linhas da tabela atual
            /// </summary>
            /// <param name="row">Linha para remoção</param>

            public void Remove(TableRow row)
            {
                if (row != null)
                    _rows.Remove(row);
            }

            /// <summary>
            /// Remove uma linha do esquema de linhas da tabela atual
            /// </summary>
            /// <param name="index">Index da <see cref="TableRow"/> para remoção</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _rows.Count - 1)
                {
                    _rows.RemoveAt(index);
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(index), Constants.INDEX_OUT_OF_RANGE_MESSAGE);
            }
        }

        /// <summary>
        /// Representa a coleção de colunas de um <see cref="Table"/>
        /// </summary>

        public class Column
        {
            private readonly List<TableColumn> _columns = new List<TableColumn>();
            private readonly int _columnCount;
            private int _addColumns = 0;

            /// <summary>
            /// Obtém ou define a cor de fundo da coluna
            /// </summary>
            public Brush BackColor { get; set; } = Brushes.LightGray;
            /// <summary>
            /// Obtém ou define a cor para conteúdos da coluna <br/> OBS: Apenas para <see cref="IFiscoComponent"/> do tipo <see cref="Text"/>
            /// </summary>
            public Brush ForeGroundColor { get; set; } = Brushes.Black;
            /// <summary>
            /// Define a fonte do cabeçalho
            /// </summary>
            public Font HeaderFont { get; set; } = new Font("Consolas", 12f);

            /// <summary>
            /// Cria um conjunto de n colunas
            /// </summary>
            /// <param name="columnCount">Quantidade de colunas</param>

            public Column(int columnCount)
            {
                _columnCount = columnCount;
            }

            /// <summary>
            /// Obtém uma coleção com todas as colunas
            /// </summary>
            /// <returns></returns>

            public ICollection<TableColumn> GetColumns()
            {
                return _columns;
            }

            /// <summary>
            /// Adiciona uma nova coluna ao esquema de colunas da tabela atual
            /// </summary>
            /// <param name="column">Nova coluna</param>
            /// <exception cref="ColumnOutOfMarginException"></exception>

            public void Add(TableColumn column)
            {
                if (_addColumns < _columnCount)
                {
                    _columns.Add(column);
                    _addColumns++;
                }
                else
                    throw new ColumnOutOfMarginException(TableConstants.MAX_COLUMN_ITENS_EXCEDED_MESSAGE.Replace("arg0", _columnCount.ToString()));
            }

            /// <summary>
            /// Remove uma coluna do esquema de colunas da tabela atual
            /// </summary>
            /// <param name="column">Coluna para remoção</param>

            public void Remove(TableColumn column)
            {
                if (_columns.Remove(column))
                    _addColumns--;
            }

            /// <summary>
            /// Remove uma coluna do esquema de colunas da tabela atual
            /// </summary>
            /// <param name="index">Index da <see cref="TableColumn"/> para remoção</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _columns.Count - 1)
                {
                    _columns.RemoveAt(index);
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(index), Constants.INDEX_OUT_OF_RANGE_MESSAGE);
            }
        }
    }

    /// <summary>
    /// Representa uma célula de uma tabela com um componente específico e uma cor de fundo.
    /// </summary>
    public class TableCell
    {
        /// <summary>
        /// Obtém ou define a cor de fundo da célula.
        /// </summary>
        public BackColor CellBackColor { get; set; }
        /// <summary>
        /// Obtém o componente associado à célula.
        /// </summary>
        public IFiscoComponent Component { get; private set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TableCell"/> com o componente especificado e cor de fundo padrão (None).
        /// </summary>
        /// <param name="component">O componente associado à célula.</param>
        public TableCell(IFiscoComponent component)
        {
            Component = component;
            CellBackColor = BackColor.None;
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TableCell"/> com o componente e cor de fundo especificados.
        /// </summary>
        /// <param name="component">O componente associado à célula.</param>
        /// <param name="backColor">A cor de fundo da célula.</param>
        public TableCell(IFiscoComponent component, BackColor backColor)
        {
            CellBackColor = backColor;
            Component = component;
        }

        /// <summary>
        /// Obtém o pincel <see cref="Brush"/> correspondente à cor de fundo da célula.
        /// </summary>
        /// <returns>O pincel correspondente à cor de fundo da célula.</returns>
        public Brush GetBrush()
        {
            switch (CellBackColor)
            {
                case BackColor.None:
                default:
                    return Brushes.White;
                case BackColor.Black:
                    return Brushes.Black;
                case BackColor.LightGray:
                    return Brushes.LightGray;
                case BackColor.DarkGray:
                    return Brushes.DarkGray;

            }
        }


        /// <summary>
        /// Enumeração das cores de fundo possíveis para a célula.
        /// </summary>
        [Flags]
        public enum BackColor
        {
            /// <summary>
            /// Nenhuma cor de fundo.
            /// </summary>
            None,

            /// <summary>
            /// Cor de fundo clara.
            /// </summary>
            LightGray,

            /// <summary>
            /// Cor de fundo cinza.
            /// </summary>
            Gray,

            /// <summary>
            /// Cor de fundo cinza escuro.
            /// </summary>
            DarkGray,

            /// <summary>
            /// Cor de fundo preta.
            /// </summary>
            Black
        }
    }

    /// <summary>
    /// Representa uma coluna em uma tabela, com nome da coluna e nome de exibição da coluna.
    /// </summary>
    public class TableColumn
    {
        /// <summary>
        /// Obtém o nome da coluna.
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// Obtém o nome de exibição da coluna.
        /// </summary>
        public string ColumnDisplayName { get; private set; }
        /// <summary>
        /// Obtém ou define um valor que indica se a cor de fundo deve ser desenhada para a coluna.
        /// </summary>
        public bool DrawBackColor { get; set; } = true;


        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TableColumn"/> com o nome da coluna especificado.
        /// </summary>
        /// <param name="columnName">O nome da coluna.</param>
        public TableColumn(string columnName)
        {
            ColumnName = columnName;
            ColumnDisplayName = columnName;
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TableColumn"/> com o nome da coluna e nome de exibição da coluna especificados.
        /// </summary>
        /// <param name="columnName">O nome da coluna.</param>
        /// <param name="columnDisplayName">O nome de exibição da coluna.</param>
        public TableColumn(string columnName, string columnDisplayName)
        {
            ColumnName = columnName;
            ColumnDisplayName = columnDisplayName;
        }
    }

    /// <summary>
    /// Representa uma linha em uma tabela, contendo células e métodos para manipulação dessas células.
    /// </summary>
    public class TableRow
    {
        private readonly List<TableCell> _cells = new List<TableCell>();
        private readonly int _maxCellsCount = -1;
        private int _addRows = 0;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="TableRow"/> com o número máximo de células especificado.
        /// </summary>
        /// <param name="columnsCount">O número máximo de células na linha.</param>
        public TableRow(int columnsCount)
        {
            _maxCellsCount = columnsCount;
        }

        /// <summary>
        /// Altera a cor de fundo de todas as células da linha.
        /// </summary>
        /// <param name="color">A cor de fundo desejada.</param>
        /// <returns>A própria instância da linha.</returns>
        public TableRow ChangeRowColor(TableCell.BackColor color)
        {
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].CellBackColor = color;

            return this;
        }

        /// <summary>
        /// Adiciona uma célula à linha.
        /// </summary>
        /// <param name="cell">A célula a ser adicionada.</param>
        public void AddCell(TableCell cell)
        {
            if (_maxCellsCount >= _addRows)
            {
                _cells.Add(cell);
                _addRows++;
            }
            else
                throw new CellTableOutOfMarginsException(TableConstants.MAX_CELL_ITENS_EXCEDED_MESSAGE.Replace("arg0", _maxCellsCount.ToString()));
        }

        /// <summary>
        /// Remove uma célula da linha.
        /// </summary>
        /// <param name="cell">A célula a ser removida.</param>
        public void RemoveCell(TableCell cell)
        {
            if (_cells.Remove(cell))
            {
                _addRows--;
            }
        }

        /// <summary>
        /// Remove uma célula da linha com base no índice.
        /// </summary>
        /// <param name="index">O índice da célula a ser removida.</param>
        public void RemoveCellAt(int index)
        {
            if (index >= 0 && index <= _cells.Count - 1)
            {
                _cells.RemoveAt(index);
                _addRows--;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(index), Constants.INDEX_OUT_OF_RANGE_MESSAGE);
        }

        /// <summary>
        /// Obtém uma lista somente leitura das células da linha.
        /// </summary>
        /// <returns>Uma lista somente leitura das células da linha.</returns>
        public IReadOnlyList<TableCell> GetCells() => _cells;
    }
}
