using Fisco;
using Fisco.Component;
using Fisco.Enumerator;
using SkiaSharp;

namespace FiscoCoreTeste
{
    public partial class Form1 : Form
    {

        private SKImage? ImagemRenderizada {  get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitObject();
        }

        private void InitObject()
        {
            Sample2();
        }

        private void Teste()
        {
            SKImage img;
            System.Drawing.Image? _bmp;

            using (FiscoPapper fisco = new(BobineSize._80x297mm, 0, 10, true))
            {
                Text H1 = new(new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)), "Teste espacamento", ItemAlign.Left, SKColors.Black);
                fisco.AddComponent(H1);

                H1 = new(new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)), "Teste espacamento", ItemAlign.Center, SKColors.Black);
                fisco.AddComponent(H1);

                H1 = new(new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)), "Teste espacamento", ItemAlign.Right, SKColors.Black);
                fisco.AddComponent(H1);

                var original = SKBitmap.Decode("D:\\a.png");
                var resized = new SKBitmap(100, 100);

                original.ScalePixels(resized, SKFilterQuality.High);

                Fisco.Component.Image logo = new Fisco.Component.Image(SKImage.FromBitmap(resized), ItemAlign.Left);
                fisco.AddComponent(logo);

                logo = new Fisco.Component.Image(SKImage.FromBitmap(resized), ItemAlign.Center);
                fisco.AddComponent(logo);

                logo = new Fisco.Component.Image(SKImage.FromBitmap(resized), ItemAlign.Right);
                fisco.AddComponent(logo);


                img = fisco.Render();
                ImagemRenderizada = img;
                var stream = img.EncodedData.AsStream();
                var bmp = Bitmap.FromStream(stream);
                _bmp = (System.Drawing.Image)bmp.Clone();
            }

            View.Image = _bmp;
        }

        private void Sample1()
        {
            SKImage img;
            System.Drawing.Image? _bmp;
            try
            {
                using (FiscoPapper fisco = new(BobineSize._80x297mm, 0, 10, true))
                {
                    //Fisco.Component.Image logo = new Fisco.Component.Image(new Bitmap(Image.FromFile("E:\\impressora.png"), new Size(40, 40)), ItemAlign.Center);
                    //fisco.AddComponent(logo);

                    Text H1 = new(new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)), "EasyLi Pro\n", ItemAlign.Center, SKColors.Black);
                    fisco.AddComponent(H1);

                    string h = $@"
USU�RIO....: Manoel Victor S. Lira
MATR�CULA..: 60179

REGISTRO..: N� 166548
DATA......: 04/03/2024
LOCAL.....: S�o Jos� do Egito - PE";

                    H1 = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), h, ItemAlign.Left, SKColors.Black);
                    fisco.AddComponent(H1);

                    H1 = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), "\n---- REGISTROS ----\n", ItemAlign.Center, SKColors.Black);
                    fisco.AddComponent(H1);


                    Table t = new(3, BobineSize._80x297mm);
                    t.SetPercentage([10, 15, 75]);
                    t.Columns.HeaderFont = new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold));

                    t.Columns.Add(new TableColumn("N�"));
                    t.Columns.Add(new TableColumn("CODE"));
                    t.Columns.Add(new TableColumn("TITLE"));


                    object[] values =
                    {
                        new object[] { "1", "2654", "O Conto da Sereia Arhto" },
                        new object[] { "2", "7896", "A Arte da Guerra"},
                        new object[] { "3", "1234", "O Senhor dos An�is"},
                        new object[] { "4", "5678", "Dom Quixote"},
                        new object[] { "5", "4321", "Cem Anos de Solid�o"},
                    };

                    foreach (object[] obj in values.Cast<object[]>())
                    {
                        TableRow row = t.GetNewRow();
                        for (int i = 0; i < 3; i++)
                            row.AddCell(new TableCell(new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), (string)obj[i], ItemAlign.Left, SKColors.Black)));

                        t.Rows.Add(row);
                    }

                    fisco.AddComponent(t);

                    var fim = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), "\nsignarute: af25s64f8eaa52\n", ItemAlign.Center, SKColors.Black); //hash: 91CFB1FAF4957E055D6CDCFF8F24821A\n
                    fisco.AddComponent(fim);
                    fim = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), "hash: 91CFB1FAF4957E055D6CDCFF8F24821A", ItemAlign.Center, SKColors.Black);
                    fisco.AddComponent(fim);

                    img = fisco.Render();
                    var stream = img.EncodedData.AsStream();
                    var bmp = Bitmap.FromStream(stream);
                    _bmp = (System.Drawing.Image)bmp.Clone();
                }

                View.Image = _bmp;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void Sample2()
        {
            SKImage img;
            System.Drawing.Image? _bmp;

            try
            {
                using (FiscoPapper fisco = new FiscoPapper(BobineSize._80x297mm, 0, 10, false))
                {
                    Table t = new Table(4, BobineSize._80x297mm)
                    {
                        RowWrap = true,
                        TableLineColor = SKColors.Black
                    };

                    var font = new SKFont(SKTypeface.FromFamilyName("Arial", 4, 12, SKFontStyleSlant.Upright));

                    float[] widths = new float[] { 40, 20, 20, 20 };
                    t.SetPercentage(widths);
                    t.Columns.BackColor = SKColors.White;
                    t.Columns.ForeGroundColor = SKColors.Black;
                    t.Columns.HeaderFont = font;

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
                            row.AddCell(new TableCell(new Text(font, $"({i}, {j})", ItemAlign.Left, i % 2 == 0 ? SKColors.Black : SKColors.White), i % 2 == 0 ? TableCell.BackColor.None : TableCell.BackColor.Black));

                        t.Rows.Add(row);
                    }

                    fisco.AddComponent(t);

                    img = fisco.Render();
                    ImagemRenderizada = img;

                    var stream = img.EncodedData.AsStream();
                    var bmp = Bitmap.FromStream(stream);
                    _bmp = (System.Drawing.Image)bmp.Clone();
                }

                View.Image = _bmp;  
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void Salvar_Click(object sender, EventArgs e)
        {
            if (ImagemRenderizada == null)
                return;

            using (var dados = ImagemRenderizada.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite("D:\\saida.png"))
            {
                dados.SaveTo(stream);
                //stream.Flush();
                //stream.Close();
            }

            MessageBox.Show("Saved");
        }
    }
}
