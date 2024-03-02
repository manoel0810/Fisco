using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FiscoTeste.Utility
{
    /// <summary>
    /// Fornece recursos para manuseio de imagens
    /// </summary>

    public class ImageProcessor
    {
        private static ImageProcessor instance;

        /// <summary>
        /// Retorna uma instância do <b>ImageProcessor</b>
        /// </summary>
        /// <returns></returns>

        public static ImageProcessor GetInstance()
        {
            if (instance == null) { instance = new ImageProcessor(); }
            return instance;
        }

        private ImageProcessor() { }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }

            return null;
        }

        /// <summary>
        /// Salva uma imagem no disco no formato JPEG
        /// </summary>
        /// <param name="image">Imagem que será salva</param>
        /// <param name="savePath">Path completo, incluindo nome do arquivo</param>

        public void SaveImage(Image image, string savePath)
        {

            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            try
            {
                Bitmap myBitmap;
                ImageCodecInfo myImageCodecInfo;
                Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;

                // Create a Bitmap object based on a BMP file.
                myBitmap = new Bitmap(image);

                // Get an ImageCodecInfo object that represents the JPEG codec.
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
                myEncoder = Encoder.Quality;
                myEncoderParameters = new EncoderParameters(1);

                // Save the bitmap as a JPEG file with quality level 75.
                myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                myBitmap.Save($"{savePath}.jpg", myImageCodecInfo, myEncoderParameters);

            }
            catch (Exception e)
            {
                MessageBox.Show($"Ocorreu um erro ao salvar a a imagem. Erro: {e.Message}", "Registro de livros", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
    }
}
