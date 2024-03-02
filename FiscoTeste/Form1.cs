using Fisco;
using Fisco.Component;
using Fisco.Enumerator;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Image = System.Drawing.Image;
namespace FiscoTeste
{
    public partial class Form1 : Form
    {
        Bitmap image;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            InitObject();
        }

        private void InitObject()
        {
            Bitmap img;

            try
            {
                using (FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, 0, 10, true))
                {

                    Table t = new Table(4, BobineSize._80x297mm)
                    {
                        RowWrap = true,
                        TableLineColor = Pens.Black
                    };

                    float[] widths = new float[] { 40, 20, 20, 20 };
                    t.SetPercentage(widths);
                    t.Columns.BackColor = Brushes.White;
                    t.Columns.ForeGroundColor = Brushes.Black;
                    t.Columns.HeaderFont = new Font("Consolas", 12f);

                    for (int i = 0; i < t.ColumnCount; i++)
                    {
                        TableColumn tc = new TableColumn($"COL {i + 1}")
                        {
                            DrawBackColor = true,
                        };

                        t.Columns.Add(tc);
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        TableRow row = new TableRow(4);
                        for (int j = 0; j < 4; j++)
                            row.AddCell(new TableCell(new Text(new Font("Arial", 8f), $"Cell {j + 1}", ItemAlign.Left, Brushes.Black), i % 2 == 0 ? TableCell.BackColor.None : TableCell.BackColor.LightGray));

                        t.Rows.Add(row);
                    }

                    fisco.AddComponent(t);
                    img = fisco.Render();
                    img = new Bitmap(img);
                }

                pictureBox1.Image = img;
                image = img;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void PrintPage(Image i)
        {
            PaperSize papel = new PaperSize("Custom Size", 80, 297); // 80mm de largura
            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.PaperSize = papel;
            doc.PrintPage += (sender, e) =>
            {
                e.Graphics.DrawImage(i, new PointF(0, 0));
            };


            doc.Print();
        }

        private void PintBtn_Click(object sender, EventArgs e)
        {
            if (image != null)
                PrintPage(image);
        }
    }
}
