using System;
using System.Windows.Forms;

namespace FiscoTeste
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--export-images")
            {
                ExportImages();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void ExportImages()
        {
            string outDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_output");
            System.IO.Directory.CreateDirectory(outDir);

            // Sample 2
            using (var fisco = new Fisco.FiscoPapper(Fisco.Enumerator.BobineSize._80x297mm, 0, 10, false))
            {
                var t = new Fisco.Component.Table(4, Fisco.Enumerator.BobineSize._80x297mm)
                {
                    RowWrap = true,
                    TableLineColor = System.Drawing.Pens.Black
                };

                float[] widths = new float[] { 40, 20, 20, 20 };
                t.SetPercentage(widths);
                t.Columns.BackColor = System.Drawing.Brushes.White;
                t.Columns.ForeGroundColor = System.Drawing.Brushes.Black;
                t.Columns.HeaderFont = new System.Drawing.Font("Consolas", 12f);

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
                            new Fisco.Component.Text(new System.Drawing.Font("Arial", 12f), $"({i}, {j})", Fisco.Enumerator.ItemAlign.Left, i % 2 == 0 ? System.Drawing.Brushes.Black : System.Drawing.Brushes.White),
                            i % 2 == 0 ? Fisco.Component.TableCell.BackColor.None : Fisco.Component.TableCell.BackColor.Black));
                    }
                    t.Rows.Add(row);
                }

                fisco.AddComponent(t);
                var img = fisco.Render();
                img.Save(System.IO.Path.Combine(outDir, "framework_sample2.png"), System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"Framework Sample2 exported: {img.Width}x{img.Height}");
            }
        }
    }
}
