using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions;
using Fisco.Exceptions.Table.Cells;
using Fisco.Exceptions.Table.Columns;
using Fisco.Exceptions.Table.Rows;
using Fisco.Utility.Constants;
using Fisco.Utility.Constants.Specific;
using SkiaSharp;
using System.Drawing;

namespace Fisco.Component
{
    /// <summary>
    /// Componente para representação de tabelas com suporte para SkiaSharp
    /// </summary>
    public class Table : IFiscoComponent, IDisposable, IDrawable
    {
        private readonly BobineSize _size;
        private int _tableRealHeight = 0;

        /// <summary>
        /// Define a quebra automática de linhas do cabeçalho
        /// </summary>
        public bool RowWrap { get; set; } = false;
        /// <summary>
        /// Define o número de colunas da tabela
        /// </summary>
        public int ColumnCount { get; private set; }
        /// <summary>
        /// Obtém a porcentagem de cada coluna com relação à largura disponível
        /// </summary>
        public float[]? UsePercentage { get; private set; }
        /// <summary>
        /// Define a cor da linha da tabela
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

        private int _currentXPosition = 0;
        private int _currentYPosition = 0;

        private Point GetCurrentPosition() => new(_currentXPosition, _currentYPosition);

        /// <summary>
        /// Cria um novo elemento de tabela
        /// </summary>
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
        /// Retorna uma nova <see cref="TableRow"/>
        /// </summary>
        public TableRow GetNewRow() => new(ColumnCount);

        /// <summary>
        /// Define a porcentagem de cada coluna com relação à largura disponível
        /// </summary>
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
            GC.SuppressFinalize(this);
        }

        private static SKSize EstimateCharSizeOnBitmap(string text, SKFont font)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return SKSize.Empty;

            using var paint = new SKPaint { Typeface = font.Typeface, TextSize = font.Size };
            var metrics = paint.FontMetrics;
            float w = paint.MeasureText(text);
            float h = metrics.Descent - metrics.Ascent;
            return new SKSize(w, h);
        }

        private void UpdateXPosition(SKRect rectangle)
        {
            _currentXPosition += Math.Abs((int)rectangle.Width);
        }

        private void NextRow(SKRect rectangle, int startX)
        {
            _currentXPosition = startX;
            _currentYPosition += (int)rectangle.Height;
        }

        private void DrawFrame(SKCanvas canvas, SKRect region, TableCell? ui = null)
        {
            if (ui != null && ui.CellBackColor != TableCell.BackColor.None)
            {
                using var fillPaint = new SKPaint { Color = ui.GetBrush(), Style = SKPaintStyle.Fill };
                canvas.DrawRect(region, fillPaint);
            }

            using var strokePaint = new SKPaint
            {
                Color = TableLineColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = false
            };
            canvas.DrawRect(region, strokePaint);
        }

        private static void DrawRegion(SKCanvas canvas, SKRect region, SKColor backColor)
        {
            using var paint = new SKPaint { Color = backColor, Style = SKPaintStyle.Fill };
            canvas.DrawRect(region, paint);
        }

        private float[] CalculateColumnWidths(float totalWidth)
        {
            float[] widths = new float[ColumnCount];
            float sum = 0;
            for (int i = 0; i < ColumnCount - 1; i++)
            {
                widths[i] = (float)Math.Floor(totalWidth * (UsePercentage![i] / 100f));
                sum += widths[i];
            }
            widths[ColumnCount - 1] = totalWidth - sum;
            return widths;
        }

        private void DrawHeader(SKCanvas canvas, float[] columnWidths, int startX)
        {
            if (Columns.GetColumns().Count != ColumnCount)
                return;

            int i = 0;
            SKFont drawFont = Columns.HeaderFont;
            string[] headersText = new string[ColumnCount];
            int maxHeight = 0;

            foreach (var column in Columns.GetColumns())
            {
                string text = column.ColumnDisplayName;
                float availableSize = columnWidths[i];

                if (RowWrap)
                {
                    var size = EstimateCharSizeOnBitmap(text, drawFont);
                    if (size.Width > availableSize && text.Length > 0)
                    {
                        var unitValue = Math.Max(1f, size.Width / text.Length);
                        var charPerLine = Math.Max(1, ((int)availableSize / (int)unitValue) - 1);

                        for (int j = charPerLine; j < text.Length; j += charPerLine + 1)
                        {
                            text = text.Insert(j, "\n");
                        }
                    }
                }

                var txtSize = EstimateCharSizeOnBitmap(text, drawFont);
                headersText[i] = text;

                if (txtSize.Height > maxHeight)
                    maxHeight = (int)Math.Ceiling(txtSize.Height);

                i++;
            }

            int headerPadding = 8;
            maxHeight += headerPadding;
            _tableRealHeight += maxHeight;

            i = 0;
            SKRect lastRec = SKRect.Empty;
            foreach (var column in Columns.GetColumns())
            {
                var absolutePos = GetCurrentPosition();
                var rec = SKRect.Create(absolutePos.X, absolutePos.Y, columnWidths[i], maxHeight);
                lastRec = rec;

                if (column.DrawBackColor)
                    DrawRegion(canvas, rec, Columns.BackColor);

                DrawFrame(canvas, rec);
                UpdateXPosition(rec);

                Text t = new(drawFont, headersText[i], ItemAlign.Center, Columns.ForeGroundColor);
                ((IDrawable)t).DrawInsideTable(ref canvas, rec);

                i++;
            }

            NextRow(lastRec, startX);
        }

        private SKRect[] CreateGridLineRegion(int rowHeight, float[] columnWidths, int startX)
        {
            int columnCount = Columns.GetColumns().Count;
            SKRect[] regions = new SKRect[columnCount];

            for (int j = 0; j < columnCount; j++)
            {
                var currentPosition = GetCurrentPosition();
                var width = columnWidths[j];
                regions[j] = SKRect.Create(currentPosition.X, currentPosition.Y, width, rowHeight);
                UpdateXPosition(regions[j]);
            }

            NextRow(regions[0], startX);
            return regions;
        }

        private void DrawTableGrid(ref SKCanvas canvas, Context context)
        {
            int startX = context.LeftOffSet;
            int startY = context.TopOffSet + context.GetStartHeight;

            _currentXPosition = startX;
            _currentYPosition = startY;
            _tableRealHeight = 0;

            float availableWidth = context.Width - context.LeftOffSet;
            float[] columnWidths = CalculateColumnWidths(availableWidth);

            DrawHeader(canvas, columnWidths, startX);

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
                            rowHeight = (int)img.GetDim().Height;
                    }
                    else if (cell.Component is Text text)
                    {
                        float h = EstimateCharSizeOnBitmap(text.TextContent, text.TextFont).Height;
                        if (h > rowHeight)
                            rowHeight = (int)Math.Ceiling(h);
                    }
                }

                int rowPadding = 6;
                rowHeight += rowPadding;

                if (!_ignoreOutBoundsError && (_currentYPosition + rowHeight) > context.Height)
                    throw new OutOfBoundsException(FiscoConstants.NO_COMPONENT_FITS);

                var regions = CreateGridLineRegion(rowHeight, columnWidths, startX);
                int e = 0;

                foreach (SKRect rec in regions)
                {
                    var tableElement = cells[e++];
                    var uiElement = (IDrawable)tableElement.Component;

                    DrawFrame(canvas, rec, tableElement);
                    uiElement.DrawInsideTable(ref canvas, rec);
                }

                _tableRealHeight += rowHeight;
            }
        }

        void IDrawable.Draw(ref SKCanvas g, ref Context drawContext)
        {
            DrawTableGrid(ref g, drawContext);
            drawContext.UpdateHeight(_tableRealHeight);
        }

        void IDrawable.DrawInsideTable(ref SKCanvas g, SKRect region)
        {
            throw new NotSupportedException(TableConstants.NOT_SUPORTED_EXCEPTION_MESSAGE);
        }

        /// <summary>
        /// Representa a coleção de linhas de um <see cref="Table"/>
        /// </summary>
        public class Row(Table.Column model)
        {
            private readonly Column _model = model;
            private readonly List<TableRow> _rows = [];

            public ICollection<TableRow> GetRows() => _rows;

            public void Add(TableRow row)
            {
                if (row.GetCells().Count > _model.GetColumns().Count || row.GetCells().Count <= 0)
                    throw new RowException(TableConstants.INCONSISTENTE_ROW_MATCH_MESSAGE);

                _rows.Add(row);
            }

            public void Remove(TableRow row)
            {
                if (row != null)
                    _rows.Remove(row);
            }

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _rows.Count)
                    _rows.RemoveAt(index);
                else
                    throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
            }
        }

        /// <summary>
        /// Representa a coleção de colunas de um <see cref="Table"/>
        /// </summary>
        public class Column(int columnCount)
        {
            private readonly List<TableColumn> _columns = [];
            private readonly int _columnCount = columnCount;
            private int _addColumns = 0;

            public SKColor BackColor { get; set; } = SKColors.LightGray;
            public SKColor ForeGroundColor { get; set; } = SKColors.Black;
            public SKFont HeaderFont { get; set; } = new(SKTypeface.FromFamilyName("Arial"));

            public ICollection<TableColumn> GetColumns() => _columns;

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

            public void Remove(TableColumn column)
            {
                if (_columns.Remove(column))
                    _addColumns--;
            }

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _columns.Count)
                {
                    _columns.RemoveAt(index);
                    _addColumns--;
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
            }
        }
    }

    /// <summary>
    /// Representa uma célula de uma tabela
    /// </summary>
    public class TableCell
    {
        public BackColor CellBackColor { get; set; }
        public IFiscoComponent Component { get; private set; }

        public TableCell(IFiscoComponent component)
        {
            Component = component;
            CellBackColor = BackColor.None;
        }

        public TableCell(IFiscoComponent component, BackColor backColor)
        {
            CellBackColor = backColor;
            Component = component;
        }

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

        [Flags]
        public enum BackColor
        {
            None,
            LightGray,
            Gray,
            DarkGray,
            Black
        }
    }

    /// <summary>
    /// Representa uma coluna em uma tabela
    /// </summary>
    public class TableColumn
    {
        public string ColumnName { get; private set; }
        public string ColumnDisplayName { get; private set; }
        public bool DrawBackColor { get; set; } = true;

        public TableColumn(string columnName)
        {
            ColumnName = columnName;
            ColumnDisplayName = columnName;
        }

        public TableColumn(string columnName, string columnDisplayName)
        {
            ColumnName = columnName;
            ColumnDisplayName = columnDisplayName;
        }
    }

    /// <summary>
    /// Representa uma linha em uma tabela
    /// </summary>
    public class TableRow(int columnsCount)
    {
        private readonly List<TableCell> _cells = [];
        private readonly int _maxCellsCount = columnsCount;
        private int _addRows = 0;

        public TableRow ChangeRowColor(TableCell.BackColor color)
        {
            for (int i = 0; i < _cells.Count; i++)
                _cells[i].CellBackColor = color;

            return this;
        }

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

        public void RemoveCell(TableCell cell)
        {
            if (_cells.Remove(cell))
                _addRows--;
        }

        public void RemoveCellAt(int index)
        {
            if (index >= 0 && index < _cells.Count)
            {
                _cells.RemoveAt(index);
                _addRows--;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(index), FiscoConstants.INDEX_OUT_OF_RANGE_MESSAGE);
        }

        public IReadOnlyList<TableCell> GetCells() => _cells;
    }
}
