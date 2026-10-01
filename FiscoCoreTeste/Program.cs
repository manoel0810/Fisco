namespace FiscoCoreTeste
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--export-images")
            {
                ExportImages();
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void ExportImages()
        {
            string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_output");
            Directory.CreateDirectory(outDir);

            // 1. TesteComprovante
            string bookCodeStr = "123456";
            string userCodeStr = "654870";

            using (var papper = new Fisco.FiscoPapper(Fisco.Enumerator.BobineSize._80x297mm, 0, 36, true))
            {
                var font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Consolas", 4, 8, SkiaSharp.SKFontStyleSlant.Upright), 36);
                var actionText = new Fisco.Component.Text(font, "EMPRÉSTIMO", Fisco.Enumerator.ItemAlign.Center, SkiaSharp.SKColors.Black);
                papper.AddComponent(actionText);

                font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Consolas", 4, 8, SkiaSharp.SKFontStyleSlant.Upright), 18);
                var data = new Dictionary<string, string>
                {
                    { "TÍTULO......:", "HUAWEI" },
                    { "CÓDIGO......:", bookCodeStr },
                    { "SOLICITANTE.:", "Derick Calado de Queiroz" },
                    { "MATRÍCULA...:", userCodeStr },
                    { "USUÁRIO.....:", "dc.queiroz" },
                    { "DATA HORA...:", "30/09/2026" },
                    { "DEVOLUÇÃO...:", "12/10/2026" }
                };

                foreach (var kvp in data)
                {
                    var info = new Fisco.Component.Text(font, $"{kvp.Key} {kvp.Value}", Fisco.Enumerator.ItemAlign.Left, SkiaSharp.SKColors.Black);
                    papper.AddComponent(info);
                }

                var barcode1 = new BarcodeStandard.Barcode { IncludeLabel = true };
                var img1 = barcode1.Encode(BarcodeStandard.Type.Code128, bookCodeStr, 200, 80);
                papper.AddComponent(new Fisco.Component.Image(img1, Fisco.Enumerator.ItemAlign.Center));

                var barcode2 = new BarcodeStandard.Barcode { IncludeLabel = true };
                var img2 = barcode2.Encode(BarcodeStandard.Type.Code128, userCodeStr, 200, 80);
                papper.AddComponent(new Fisco.Component.Image(img2, Fisco.Enumerator.ItemAlign.Center));

                var rendered = papper.Render();
                using var fs = File.OpenWrite(Path.Combine(outDir, "core_comprovante.png"));
                rendered.EncodedData.SaveTo(fs);
                Console.WriteLine($"Comprovante exported: {rendered.Width}x{rendered.Height}");
            }

            // 2. Sample2 (Table)
            using (var fisco = new Fisco.FiscoPapper(Fisco.Enumerator.BobineSize._80x297mm, 0, 10, false))
            {
                var t = new Fisco.Component.Table(4, Fisco.Enumerator.BobineSize._80x297mm)
                {
                    RowWrap = true,
                    TableLineColor = SkiaSharp.SKColors.Black
                };

                var font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Arial", 4, 8, SkiaSharp.SKFontStyleSlant.Upright), 16);
                float[] widths = [40, 20, 20, 20];
                t.SetPercentage(widths);
                t.Columns.BackColor = SkiaSharp.SKColors.White;
                t.Columns.ForeGroundColor = SkiaSharp.SKColors.Black;
                t.Columns.HeaderFont = font;

                for (int i = 0; i < t.ColumnCount; i++)
                {
                    t.Columns.Add(new Fisco.Component.TableColumn($"COL {i + 1}") { DrawBackColor = true });
                }

                for (int i = 0; i < 20; i++)
                {
                    var row = new Fisco.Component.TableRow(4);
                    for (int j = 0; j < 4; j++)
                    {
                        row.AddCell(new Fisco.Component.TableCell(
                            new Fisco.Component.Text(font, $"({i}, {j})", Fisco.Enumerator.ItemAlign.Left, i % 2 == 0 ? SkiaSharp.SKColors.Black : SkiaSharp.SKColors.White),
                            i % 2 == 0 ? Fisco.Component.TableCell.BackColor.None : Fisco.Component.TableCell.BackColor.Black));
                    }
                    t.Rows.Add(row);
                }

                fisco.AddComponent(t);
                var rendered = fisco.Render();
                using var fs = File.OpenWrite(Path.Combine(outDir, "core_sample2.png"));
                rendered.EncodedData.SaveTo(fs);
                Console.WriteLine($"Sample2 Table exported: {rendered.Width}x{rendered.Height}");
            }

            // 3. Sample1 (Multiline text + table)
            using (var fisco = new Fisco.FiscoPapper(Fisco.Enumerator.BobineSize._80x297mm, 0, 10, true))
            {
                var fontHeader = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Consolas", 4, 8, SkiaSharp.SKFontStyleSlant.Upright), 24);
                var h1 = new Fisco.Component.Text(fontHeader, "EasyLi Pro\n", Fisco.Enumerator.ItemAlign.Center, SkiaSharp.SKColors.Black);
                fisco.AddComponent(h1);

                string h = @"USUÁRIO....: Manoel Victor S. Lira
MATRÍCULA..: 60179

REGISTRO..: Nº 166548
DATA......: 04/03/2024
LOCAL.....: São José do Egito - PE";

                var fontBody = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Consolas", 4, 8, SkiaSharp.SKFontStyleSlant.Upright), 14);
                fisco.AddComponent(new Fisco.Component.Text(fontBody, h, Fisco.Enumerator.ItemAlign.Left, SkiaSharp.SKColors.Black));

                var rendered = fisco.Render();
                using var fs = File.OpenWrite(Path.Combine(outDir, "core_sample1.png"));
                rendered.EncodedData.SaveTo(fs);
                Console.WriteLine($"Sample1 Multiline exported: {rendered.Width}x{rendered.Height}");
            }
        }
    }
}