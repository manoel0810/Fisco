using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using Fisco.Exceptions.Table.Cells;
using Fisco.Exceptions.Table.Columns;
using Fisco.Exceptions.Table.Rows;
using Fisco.Utility;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace Fisco.Component
{
    /// <summary>
    /// Componente para representação de tabelas
    /// </summary>

    public class Table : IFiscoComponent, IDrawable
    {
        private SKBitmap? _tableBitmap;
        private SKCanvas? _tableGraphics;
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
        public float[]? UsePercentage { get; private set; }
        /// <summary>
        /// Define o pincel para desenhar o cabeçalho
        /// </summary>
        public SKColor TableLineColor { get; set; } = SKColors.Black;

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

        /// <summary>
        /// X == right Y == bottom
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Point GetRightBottomAbsolutePosition(float width, float height)
        {
            float bottom = _tableBitmap!.Height - (_currentYPosition + height);
            float right = _tableBitmap.Width - (_currentXPosition + width);

            return new Point((int)right, (int)bottom);
        }

        //private Point GetNewPointFromVector(Point unit) => new Point(_currentXPosition - unit.X, _currentYPosition - unit.Y);

        //---------------------------------------------------------------------------------//


        /// <summary>
        /// Cria um novo elemento gráfico do tipo <see cref="IFiscoComponent"/> para renderização com suporte para tabelas
        /// </summary>
        /// <param name="columnsCount">Número total de colunas</param>
        /// <param name="size">Tipo da bobina </param>
        /// <param name="ignoreOutBoundsError">Quando true, ignora áreas fora dos limites de desenho</param>
        /// <exception cref="FiscoException"></exception>

        public Table(int columnsCount, BobineSize size, bool ignoreOutBoundsError = false)
        {
            if (columnsCount < TableConstants.MIN_TABLE_COLUMNS_COUNT)
                throw new FiscoException(TableConstants.MIN_TABLE_COLUMN_COUNT_MESSAGE, new ArgumentOutOfRangeException(nameof(columnsCount)));

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

            if (widths.Sum() < (float)TableConstants.MAX_WIDTH_PERCENTAGE)
                throw new InvalidWidthsColumnException(TableConstants.SUM_PERCENTAGE_MIN_MESSAGE);

            UsePercentage = widths;
        }

        void IDisposable.Dispose()
        {
            using(StreamWriter writer = new StreamWriter("D:\\log.txt"))
            {
                dados.ForEach(x => writer.WriteLine(x));
                writer.Flush();
                writer.Close();
            }

            GC.SuppressFinalize(this);
            _tableGraphics?.Dispose();
            _tableBitmap?.Dispose();
        }

        private void InitBitmap()
        {
            _tableBitmap = GraphicsGenerator.GenerateBitmapField(new Context(_size, _ignoreOutBoundsError));
            _tableGraphics = GraphicsGenerator.GenerateGraphicsObject(ref _tableBitmap, SKColors.White);
        }

        private static SKSize EstimateCharSizeOnBitmap(string text, SKFont font)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return SKSize.Empty;

            using (var paint = new SKPaint { Typeface = font.Typeface, TextSize = font.Size })
            {
                return new SKSize(paint.MeasureText(text), paint.FontMetrics.CapHeight);
            }
        }

        private void UpdateXPosition(SKRect rectangle)
        {
            _currentXPosition += Math.Abs((int)rectangle.Width);
        }

        private void NextRow(SKRect rectangle)
        {
            _currentXPosition = 0;
            _currentYPosition += (int)rectangle.Height;
        }

        private void DrawFrame(SKRect region, TableCell? ui = null)
        {
            SKPoint[] points =
            {
                new(Math.Abs(region.Left), Math.Abs(region.Top)),
                new(Math.Abs(region.Left), Math.Abs(region.Bottom)),
                new(Math.Abs(region.Right), Math.Abs(region.Bottom)),
                new(Math.Abs(region.Right), Math.Abs(region.Top)),
                new(Math.Abs(region.Left), Math.Abs(region.Top)),
            };

            if (ui != null)
            {
                using (var paint = new SKPaint { Color = ui.GetBrush(), Style = SKPaintStyle.Fill })
                {
                    _tableGraphics!.DrawRect(region, paint);
                };
            }

            using (var paint = new SKPaint { Color = TableLineColor, Style = SKPaintStyle.Stroke })
            {
                _tableGraphics!.DrawPoints(SKPointMode.Polygon, points, paint);
            }
        }

        private void DrawRegion(SKRect region, SKColor backColor)
        {
            using (var paint = new SKPaint { Color = backColor, Style = SKPaintStyle.Fill })
            {
                _tableGraphics!.DrawRect(region, paint);
            }
        }

        private float GetRealWidth(bool incluseSecurityMargin = true)
        {
            return BobineProps.GetSizesUsingPPI(_size)[0] - (incluseSecurityMargin ? TableConstants.SECURITY_MARGIN : 0);
        }

        private float GetRealSizeByPercentage(float percentage)
        {
            return (GetRealWidth() * percentage) / (float)TableConstants.MAX_WIDTH_PERCENTAGE;
        }

        private void DrawComponent(IDrawable component, SKRect region)
        {
            component.DrawInsideTable(ref _tableGraphics!, region);
        }

        private static List<string> dados = [];
        private static void Log(string text)
        {
            dados.Add(text);
        }

        private void DrawHeader()
        {
            if (Columns.GetColumns().Count != ColumnCount)
                return;

            int i = 0;
            SKFont drawFont = Columns.HeaderFont;
            int[] avaibleColumnSizes = new int[ColumnCount];
            string[] headersText = new string[ColumnCount];
            int maxHeight = 0;

            foreach (var column in Columns.GetColumns())
            {
                string text = column.ColumnDisplayName;
                var avaibleSize = GetRealSizeByPercentage(UsePercentage![i]);

                Log($"TEXT: ({text}) || SizeRow → {avaibleSize}");

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
            maxHeight *= 2;
            _tableRealHeight += maxHeight;
            foreach (var column in Columns.GetColumns())
            {
                var absolutePos = GetCurrentPosition();
                var rec = new SKRect(absolutePos.X, absolutePos.Y, absolutePos.X + avaibleColumnSizes[i], maxHeight);

                var debug = $"ABS_POS: ({absolutePos}) || Rect → (left:{rec.Left}, top:{rec.Top}, right:{rec.Right}, bottom:{rec.Bottom})";
                System.Diagnostics.Debug.WriteLine(debug);
                Log(debug);

                if (column.DrawBackColor)
                    DrawRegion(rec, Columns.BackColor);

                DrawFrame(rec);
                UpdateXPosition(rec);

                Text t = new(drawFont, headersText[i], ItemAlign.Center, Columns.ForeGroundColor);
                DrawComponent(t, rec);

                i++;
                if (i >= UsePercentage!.Length)
                {
                    NextRow(rec);
                    i = 0;
                }
            }
        }

        private SKRect[] CreateGridLineRegion(int maxHeight)
        {
            int columnCount = Columns.GetColumns().Count;
            SKRect[] regions = new SKRect[columnCount];

            for (int j = 0; j < columnCount; j++)
            {
                var currentPosition = GetCurrentPosition();
                var width = (float)GetRealSizeByPercentage(UsePercentage![j]);
                regions[j] = new SKRect(currentPosition.X, currentPosition.Y, currentPosition.X + width, currentPosition.Y + maxHeight);

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
                            rowHeight = (int)img.GetDim().Height;
                        }
                    }
                    else if (cell.Component is Text text)
                    {
                        float h = EstimateCharSizeOnBitmap(text.TextContent, text.TextFont).Height;
                        if (h > rowHeight)
                            rowHeight = (int)h;
                    }
                }

                rowHeight *= 2;
                var regions = CreateGridLineRegion(rowHeight);
                int e = 0;

                if (!_ignoreOutBoundsError && rowHeight > _tableBitmap!.Height - _tableRealHeight)
                    throw new OutOfBoundsException(FiscoConstants.NO_COMPONENT_FITS);

                foreach (SKRect rec in regions)
                {
                    var tableElement = cells[e++];
                    var uiElement = (IDrawable)(tableElement.Component);

                    DrawFrame(rec, tableElement);
                    DrawComponent(uiElement, rec );
                }

                _tableRealHeight += rowHeight;
            }
        }

        void IDrawable.Draw(ref SKCanvas g, ref Context drawContext)
        {
            DrawTableGrid();
            g.DrawImage(SKImage.FromBitmap(_tableBitmap), 0, drawContext.GetStartHeight + drawContext.TopOffSet);
            drawContext.UpdateHeight(_tableRealHeight + drawContext.TopOffSet);
        }

        void IDrawable.DrawInsideTable(ref SKCanvas g, SKRect region)
        {
            throw new NotSupportedException(TableConstants.NOT_SUPORTED_EXCEPTION_MESSAGE);
        }

        /// <summary>
        /// Representa a coleção de linhas de um <see cref="Table"/>
        /// </summary>
        /// <remarks>
        /// Cria uma nova <see cref="Row"/> com base no modelo de colunas
        /// </remarks>
        /// <param name="model">Modelo de colunas</param>

        public class Row(Table.Column model)
        {
            private readonly Column _model = model;
            private readonly List<TableRow> _rows = [];

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
                    throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
            }
        }

        /// <summary>
        /// Representa a coleção de colunas de um <see cref="Table"/>
        /// </summary>
        /// <remarks>
        /// Cria um conjunto de n colunas
        /// </remarks>
        /// <param name="columnCount">Quantidade de colunas</param>

        public class Column(int columnCount)
        {
            private readonly List<TableColumn> _columns = [];
            private readonly int _columnCount = columnCount;
            private int _addColumns = 0;

            /// <summary>
            /// Obtém ou define a cor de fundo da coluna
            /// </summary>
            public SKColor BackColor { get; set; } = SKColors.LightGray;
            /// <summary>
            /// Obtém ou define a cor para conteúdos da coluna <br/> OBS: Apenas para <see cref="IFiscoComponent"/> do tipo <see cref="Text"/>
            /// </summary>
            public SKColor ForeGroundColor { get; set; } = SKColors.Black;
            /// <summary>
            /// Define a fonte do cabeçalho
            /// </summary>
            public SKFont HeaderFont { get; set; } = new SKFont(SKTypeface.FromFamilyName("Arial"));

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
                    throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
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
        public SKColor GetBrush()
        {
            return CellBackColor switch
            {
                BackColor.Black => SKColors.Black,
                BackColor.LightGray => SKColors.LightGray,
                BackColor.DarkGray => SKColors.DarkGray,
                _ => SKColors.White,
            };
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
    /// <remarks>
    /// Inicializa uma nova instância da classe <see cref="TableRow"/> com o número máximo de células especificado.
    /// </remarks>
    /// <param name="columnsCount">O número máximo de células na linha.</param>
    public class TableRow(int columnsCount)
    {
        private readonly List<TableCell> _cells = [];
        private readonly int _maxCellsCount = columnsCount;
        private int _addRows = 0;

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
                throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
        }

        /// <summary>
        /// Obtém uma lista somente leitura das células da linha.
        /// </summary>
        /// <returns>Uma lista somente leitura das células da linha.</returns>
        public IReadOnlyList<TableCell> GetCells() => _cells;
    }
}
