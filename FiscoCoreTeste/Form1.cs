using Fisco;
using Fisco.Component;
using Fisco.Enumerator;
using SkiaSharp;

namespace FiscoCoreTeste
{
    public partial class Form1 : Form
    {
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
            Sample1();
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
USUÁRIO....: Manoel Victor S. Lira
MATRÍCULA..: 60179

REGISTRO..: Nº 166548
DATA......: 04/03/2024
LOCAL.....: São José do Egito - PE";

                    H1 = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), h, ItemAlign.Left, SKColors.Black);
                    fisco.AddComponent(H1);

                    H1 = new Text(new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold)), "\n---- REGISTROS ----\n", ItemAlign.Center, SKColors.Black);
                    fisco.AddComponent(H1);


                    Table t = new(3, BobineSize._80x297mm);
                    t.SetPercentage([10, 15, 75]);
                    t.Columns.HeaderFont = new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold));

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
    }
}
