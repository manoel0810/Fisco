using Fisco;
using Fisco.Component;
using Fisco.Enumerator;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
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
            Sample2();
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

        private void Sample1()
        {
            Bitmap img;

            try
            {
                using (FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, 0, 10, true))
                {
                    Fisco.Component.Image logo = new Fisco.Component.Image(new Bitmap(Image.FromFile("E:\\impressora.png"), new Size(40, 40)), ItemAlign.Center);
                    fisco.AddComponent(logo);

                    Fisco.Component.Text H1 = new Text(new Font("Consolas", 8f, FontStyle.Bold, GraphicsUnit.Millimeter), "EasyLi Pro\n", ItemAlign.Center, Brushes.Black);
                    fisco.AddComponent(H1);

                    string h = $@"
USUÁRIO....: Manoel Victor S. Lira
MATRÍCULA..: 60179

REGISTRO..: Nº 166548
DATA......: 04/03/2024
LOCAL.....: São José do Egito - PE";

                    H1 = new Text(new Font("Consolas", 10f, FontStyle.Bold), h, ItemAlign.Left, Brushes.Black);
                    fisco.AddComponent(H1);

                    H1 = new Text(new Font("Consolas", 8f), "\n---- REGISTROS ----\n", ItemAlign.Center, Brushes.Black);
                    fisco.AddComponent(H1);


                    Table t = new Table(3, BobineSize._80x297mm);
                    t.SetPercentage(new float[] { 10, 15, 75 });
                    t.Columns.HeaderFont = new Font("Consolas", 4f, FontStyle.Bold, GraphicsUnit.Millimeter);

                    t.Columns.Add(new TableColumn("Nº"));
                    t.Columns.Add(new TableColumn("CODE"));
                    t.Columns.Add(new TableColumn("TITLE"));


                    object[] values =
                    {
                        new object[] { "1", "2654", "O Conto da Sereia Arhto" },
                        new object[] { "2", "7896", "A Arte da Guerra"},
                        new object[] { "3", "1234", "O Senhor dos Anéis"},
                        new object[] { "4", "5678", "Dom Quixote"},
                        new object[] { "5", "4321", "Cem Anos de Solidão"},
                    };

                    foreach (object[] obj in values.Cast<object[]>())
                    {
                        TableRow row = t.GetNewRow();
                        for (int i = 0; i < 3; i++)
                            row.AddCell(new TableCell(new Text(new Font("Consolas", 10f, FontStyle.Bold), (string)obj[i], ItemAlign.Left, Brushes.Black)));

                        t.Rows.Add(row);
                    }

                    fisco.AddComponent(t);

                    var fim = new Text(new Font("Consolas", 4f, FontStyle.Bold, GraphicsUnit.Millimeter), "\nsignarute: af25s64f8eaa52\n", ItemAlign.Center, Brushes.Black); //hash: 91CFB1FAF4957E055D6CDCFF8F24821A\n
                    fisco.AddComponent(fim);
                    fim = new Text(new Font("Consolas", 3f, FontStyle.Bold, GraphicsUnit.Millimeter), "hash: 91CFB1FAF4957E055D6CDCFF8F24821A", ItemAlign.Center, Brushes.Black);
                    fisco.AddComponent(fim);

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

        private void Sample2()
        {
            Bitmap img;

            try
            {
                using (FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, 0, 10, false))
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

                    for (int i = 0; i < 100; i++)
                    {
                        TableRow row = new TableRow(4);
                        for (int j = 0; j < 4; j++)
                            row.AddCell(new TableCell(new Text(new Font("Arial", 12f), $"({i}, {j})", ItemAlign.Left, i % 2 == 0 ? Brushes.Black : Brushes.White), i % 2 == 0 ? TableCell.BackColor.None : TableCell.BackColor.Black));

                        t.Rows.Add(row);
                    }

                    fisco.AddComponent(t);
                    img = fisco.Render();
                    img = new Bitmap(img);

                    pictureBox1.Image = img;
                    image = img;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    
        private void Sample3()
        {
            Bitmap img;

            try
            {
                using(FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, false))
                {
                    Fisco.Component.Image well = new Fisco.Component.Image(new Bitmap(Image.FromFile("D:\\a.jpg"), new Size(250, 350)), ItemAlign.Center);
                    fisco.AddComponent(well);

                    img = fisco.Render();
                    img = new Bitmap(img);
                }

                pictureBox1.Image = img;
                image = img;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
    }
}
