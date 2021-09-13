using AForge.Imaging.Filters;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using Windows.UI.Xaml.Media.Imaging;


namespace HtmlPackAgilityEstudo
{
    //instalar o package HtmlPackAgility
    //link do tesseract https://github.com/tesseract-ocr/tessdata e teserract.drawning  from nuget
    //link do AForge.Net http://www.aforgenet.com/framework/downloads.html
    ///pode ser encontrada no nugt Tesseract
    ///Adicionando arquivos e referências
    ///
    ///O próximo passo é adicionarmos os arquivos e referências das ferramentas:
    ///
    ///AForge.NET: Adicionar a referência para AForge.dll e AForge.Imaging.dll (Utilizaremos apenas elas para fazer o tratamento de imagem dos nossos captchas).
    ///
    ///Tesseract: Adicionar a referência para Tesseract.dll. Adicionar na raiz do projeto a pasta “tessdata” que contém uma espécie de base de dados treinada para o reconhecimento OCR. É necessário adicionar as dll’s: liblept168.dll e libtesseract302.dll na raiz do projeto também.
    ///
    ///ATENÇÃO: Em todos os arquivos adicionados do Tesseract é necessário marcar a opção “Copy to Output Direcoty” que pode ser definido clicando sobre o arquivo dentro do Visual Studio e indo em propriedades.
    public class Robo
    {
        public  void GetImgCaptcha()
        {
            //HtmlPackAgility
            HttpClientHandler redirect = new HttpClientHandler();
            redirect.AllowAutoRedirect = true;
            HtmlDocument html = new HtmlDocument();
            HttpClient client = new HttpClient(redirect);
            Uri baseUrl = new Uri("https://www4.suframa.gov.br/");
            client.BaseAddress = baseUrl;
            client.DefaultRequestHeaders.Add("Accept", "application/*+xml;version=5.1");
            client.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:91.0) Gecko/20100101 Firefox/91.0");
            HttpResponseMessage response = new HttpResponseMessage();
            response = client.GetAsync("login.aspx").Result;
            if (response.IsSuccessStatusCode)
            {
                html.LoadHtml(response.Content.ReadAsStringAsync().Result);
                HtmlNode img = html.GetElementbyId("ContentPlaceHolder1_Image1");
                string urlCaptcha = img.GetAttributeValue("src","");
                string idCaptcha = img.Id;
                response = client.GetAsync(urlCaptcha).Result;
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        byte[] imagem =  response.Content.ReadAsByteArrayAsync().Result;
                        MemoryStream ms = new MemoryStream(imagem);
                        Console.WriteLine(reconhecerCaptcha(imagem));
                        string filePath = @"C:\Users\Public\Pictures\Imagem_Capturada.jpeg";
                        File.WriteAllBytes(filePath, imagem);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
/*
                    Stream inputStream = response.Content.ReadAsStream();
                    //Image sImg = Base64FormattingOptions()
                    //BitmapImage bitmapImage = new BitmapImage();
                    System.Drawing.Image imgs = System.Drawing.Image.FromStream(inputStream);

                    imgs.Save(@"C:\myImage.Jpeg", ImageFormat.Jpeg);
*/
                }
            }


        }
        
        private string OCR(byte[] filterIMG)
        {
            MemoryStream ms = new MemoryStream(filterIMG);
            Bitmap imagem = new Bitmap(ms);

            Pix img = PixConverter.ToPix(imagem);

            string filePath = @"C:\Users\Public\Pictures\Imagem_Filtrada.jpeg";
            File.WriteAllBytes(filePath, filterIMG);


            string res = "";

            using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default))
            {
                //engine.SetVariable("tessedit_pageseg_mode", 10);
                engine.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                engine.SetVariable("tessedit_unrej_any_wd", true);

                using (var page = engine.Process(img, PageSegMode.SingleLine))
                    res = page.GetText();
            }

            filePath = @"C:\Users\Public\Pictures\Imagem_Filtrada.txt";
            File.WriteAllText(filePath, res);

            return res;
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        private string reconhecerCaptcha(byte[] img)
        {
          
            MemoryStream ms = new MemoryStream(img);
            Bitmap imagem = new Bitmap(ms);
           
            imagem = imagem.Clone(new Rectangle(0, 0, imagem.Width, imagem.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            string filePath = @"C:\Users\Public\Pictures\Imagem_capturada.jpeg";
            File.WriteAllBytes(filePath, img);

            Erosion erosion = new Erosion();
            Dilatation dilatation = new Dilatation();
            Invert inverter = new Invert();
            ColorFiltering cor = new ColorFiltering();
            cor.Blue = new AForge.IntRange(200, 255);
            cor.Red = new AForge.IntRange(200, 255);
            cor.Green = new AForge.IntRange(200, 255);
            Opening open = new Opening();
            BlobsFiltering bc = new BlobsFiltering();
            Closing close = new Closing();
            GaussianSharpen gs = new GaussianSharpen();
            gs.Threshold = 3;
            ContrastCorrection cc = new ContrastCorrection();
            bc.MinHeight = 10;
            FiltersSequence seq = new FiltersSequence(gs, inverter, open, inverter, bc, inverter, open, cc, cor, bc, inverter);
            Bitmap imagemFiltarda = seq.Apply(imagem);
            string reconhecido = OCR(ImageToByte(imagemFiltarda));
            return reconhecido;
        }
        public Robo() {
        
        }
    }
}
