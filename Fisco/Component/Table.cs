using Fisco.Component.Interfaces;
using Fisco.Enumerator;
using Fisco.Exceptions.Table.Cells;
using Fisco.Exceptions.Table.Columns;
using Fisco.Exceptions.Table.Rows;
using Fisco.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fisco.Component
{
    public class Table : IFiscoComponent, IDrawable
    {
        //TODO: mover SECURITY_MARGIN para um escopo global
        private const int SECURITY_MARGIN = 35;

        private Bitmap _tableBitmap;
        private Graphics _tableGraphics;
        private readonly BobineSize _size;
        private int _tableRealHeight = 0;

        public bool RowWrap { get; set; } = false;
        public int ColumnCount { get; private set; }
        public float[] UsePercentage { get; private set; }
        public Pen TableLineColor { get; set; } = new Pen(Brushes.Black, 1.5f);

        private readonly bool _ignoreOutBoundsError;
        public readonly Column Columns;
        public readonly Row Rows;

        //---------------------------------------------------------------------------------//

        private int _currentXPosition = 0;
        private int _currentYPosition = 0;

        private Point GetCurrentPosition() => new Point(_currentXPosition, _currentYPosition);

        private Point GetNewPointFromVector(Point unit) => new Point(_currentXPosition - unit.X, _currentYPosition - unit.Y);

        //---------------------------------------------------------------------------------//

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
            float partValue = 100f / ColumnCount;
            float[] values = new float[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
                values[i] = partValue;

            SetPercentage(values);
        }

        public TableRow GetNewRow()
        {
            return new TableRow(ColumnCount);
        }

        public void SetPercentage(float[] widths)
        {
            if (widths.Length != ColumnCount)
                throw new InvalidWidthsColumnException("A quantidade de valores passada é diferente do número de colunas");

            if ((int)widths.Sum() > 100)
                throw new InvalidWidthsColumnException("A soma dos valores para tamanho das colunas não deve ser superior a 100%");

            UsePercentage = widths;
        }

        public void Dispose()
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

        private void UpdateYPosition(Rectangle rectangle)
        {
            _currentYPosition += rectangle.Height;
        }

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
            return BobineProps.GetSizesUsingPPI(_size)[0] - (incluseSecurityMargin ? SECURITY_MARGIN : 0);
        }

        private float GetRealSizeByPercentage(float percentage)
        {
            return (GetRealWidth() * percentage) / 100f;
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

        private Point GetStartPont()
        {
            var realSize = GetRealWidth(false);
            var sizeWithMargin = GetRealWidth();

            var startX = (realSize - sizeWithMargin) / 2;
            return new Point((int)startX - 15, _currentYPosition);
        }

        public void Draw(ref Graphics g, ref Context drawContext)
        {
            DrawTableGrid();
            g.DrawImage(_tableBitmap, new Point(GetStartPont().X, drawContext.GetStartHeight + drawContext.TopOffSet));
            drawContext.UpdateHeight(_tableRealHeight + drawContext.TopOffSet);
        }

        public void DrawInsideTable(ref Graphics g, Rectangle region)
        {
            throw new NotImplementedException();
        }

        public class Row
        {
            private readonly Column _model;
            private readonly List<TableRow> _rows = new List<TableRow>();

            public Row(Column model)
            {
                _model = model;
            }

            public ICollection<TableRow> GetRows()
            {
                return _rows;
            }

            public void Add(TableRow row)
            {
                if (row.GetCells().Count > _model.GetColumns().Count || row.GetCells().Count <= 0)
                    throw new RowException("Linha inconsistente com o modelo de tabela atual");

                _rows.Add(row);
            }

            public void Remove(TableRow row)
            {
                if (row != null)
                    _rows.Remove(row);
            }

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _rows.Count - 1)
                {
                    _rows.RemoveAt(index);
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(index), "O index deve estar dentro dos limites do array");
            }
        }

        public class Column
        {
            private readonly List<TableColumn> _columns = new List<TableColumn>();
            private readonly int _columnCount;
            private int _addColumns = 0;

            public Brush BackColor { get; set; } = Brushes.LightGray;
            public Brush ForeGroundColor { get; set; } = Brushes.Black;
            public Font HeaderFont { get; set; } = new Font("Consolas", 12f);

            public Column(int columnCount)
            {
                _columnCount = columnCount;
            }

            public ICollection<TableColumn> GetColumns()
            {
                return _columns;
            }

            public void Add(TableColumn column)
            {
                if (_addColumns < _columnCount)
                {
                    _columns.Add(column);
                    _addColumns++;
                }
                else
                    throw new ColumnOutOfMarginException($"Não é possível adicionar mais uma coluna à tabela\nLimite máximo de {_columnCount} colunas");
            }

            public void Remove(TableColumn column)
            {
                if (_columns.Remove(column))
                    _addColumns--;
            }

            public void RemoveAt(int index)
            {
                if (index >= 0 && index < _columns.Count - 1)
                {
                    _columns.RemoveAt(index);
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(index), "O index deve estar dentro dos limites do array");
            }
        }
    }

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

    public class TableRow
    {
        private readonly List<TableCell> _cells = new List<TableCell>();
        private readonly int _maxCellsCount = -1;
        private int _addRows = 0;

        public TableRow(int columnsCount)
        {
            _maxCellsCount = columnsCount;
        }

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
                throw new CellTableOutOfMarginsException($"Não é possível adicionar mais uma célula à linha\nLimite máximo de {_maxCellsCount} células");
        }

        public void RemoveCell(TableCell cell)
        {
            if (_cells.Remove(cell))
            {
                _addRows--;
            }
        }

        public void RemoveCellAt(int index)
        {
            if (index >= 0 && index <= _cells.Count - 1)
            {
                _cells.RemoveAt(index);
                _addRows--;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(index), "O index deve estar dentro dos limites do array");
        }

        public IReadOnlyList<TableCell> GetCells() => _cells;
    }
}
