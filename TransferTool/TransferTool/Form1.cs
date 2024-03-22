using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text.RegularExpressions;
using System.Xml;
using Spire.Pdf.Utilities;

using Spire;
using System.Web;

namespace TransferTool
{
    public partial class Form1 : Form
    {
        public static event EventHandler FilePathUpdated;

        FileSystemWatcher watcher;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool isWatching;

        private string defaultPath = "C:\\Windows";
        private string filePath = MyConfig.FilePath;

        private static IConfiguration? _configuration;

        public static void InitConfiguration()
        {
            //_configuration = new ConfigurationBuilder()
            //.AddJsonFile("coordinates.json", optional: false, reloadOnChange: true)
            //.Build();
        }

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = services.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<BlazorTest>("#app");
            // Subscribe to FilePathUpdated event
            MyConfig.FilePathUpdated += MyConfig_FilePathUpdated;
            startWatching();
        }

        private void startWatching()
        {
            isWatching = true;
            timer.Enabled = true;
            timer.Start();
            timer.Interval = 500;

            watcher = new FileSystemWatcher();

            //Als de pad niet bestaat, dan pakt hij de defaultPath, anders de opgegeven filePath
            if (Directory.Exists(filePath))
            {
                watcher.Path = filePath;
            }
            else
            {
                watcher.Path = defaultPath;
            }

            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;

            watcher.Created += new FileSystemEventHandler(OnCreated);

            watcher.EnableRaisingEvents = true;
        }

        private void MyConfig_FilePathUpdated(object sender, EventArgs e)
        {
            RestartWatching();
        }

        private void RestartWatching()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            filePath = MyConfig.FilePath;
            startWatching();
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            string destinationPath;

            string fileName = e.Name;
            string fileExtension = System.IO.Path.GetExtension(fileName);

            if (Directory.Exists(MyConfig.FilePath))
            {
                destinationPath = MyConfig.FilePath + "\\" + fileName;
            }
            else
            {
                destinationPath = defaultPath + "\\" + fileName;
            }


            if (fileExtension.ToLower() == ".pdf")
            {
                await Task.Delay(500);

                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int j = 10;
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        //Uitlezen van de gegevens vanuit PDF
                        StringBuilder text = new StringBuilder();
                        StringBuilder order = new StringBuilder();

                        pdfDefinition def = null;
                        xmlOrder xmlOrder = new xmlOrder();

                        using (PdfReader reader = new PdfReader(memoryStream))
                        {
                            HashSet<string> uniqueTags = new HashSet<string>();
                            HashSet<string> uniqueArtikel = new HashSet<string>();


                            //Spire om de artikelen uit te halen

                            /*Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                            doc.LoadFromFile(destinationPath);
                            StringBuilder builder = new StringBuilder();

                            PdfTableExtractor extractor = new PdfTableExtractor(doc);
                            PdfTable[] tableList = null;

                            for (int pageIndex = 0; pageIndex < reader.NumberOfPages; pageIndex++)
                            {
                                //Extract table(s) from each page into a PdfTable array
                                PdfTable[] tableLists = extractor.ExtractTable(pageIndex);

                                if (tableLists != null && tableLists.Length > 0)
                                {
                                    //Loop through tables in the PdfTable array
                                    foreach (PdfTable table in tableLists)
                                    {
                                        //Loop through each table row
                                        for (int t = 0; t < table.GetRowCount(); t++)
                                        {
                                            //Loop through each table column
                                            for (int y = 0; y < table.GetColumnCount(); y++)
                                            {
                                                //Extract data from each table cell
                                                string text1 = table.GetText(t, y);
                                                //Append data to the StringBuilder instance
                                                builder.Append(text1 + "  |  ");
                                            }
                                            builder.Append("\r\n");
                                        }
                                    }
                                }
                            }*/

                            for (int i = 1; i <= reader.NumberOfPages; i++)
                            { 
                                //Definieren alle text van PDF
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                                string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                                MyConfig.InitConfig();
                                def = MyConfig.GetDefinition(currentText);

                                if (def != null)
                                { 
                                    text.Append(currentText);

                                    foreach (var defObject in def.defObjects)
                                    {
                                        var value = defObject.GetValue(reader, currentText, i, defObject, j);

                                        //XmlNiveau wordt van tevoren gedefinieerd. Als het een Order is, voeg toe aan gegevens. Anders is het een artikel.
                                        if (defObject.XmlNiveau == XmlNiveau.Order)
                                        {
                                            //Als de Tags al bestaat in xmlOrder, maak dan geen nieuwe, anders wel.
                                            if (!uniqueTags.Contains(defObject.TagNaam) && !string.IsNullOrEmpty(defObject.Value))
                                            {
                                                uniqueTags.Add(defObject.TagNaam);
                                                xmlOrder.Items.Add(defObject);
                                            }
                                        }
                                        else
                                        {
                                            if (defObject.Value != null)
                                            {
                                                string[] EachOrder = defObject.Value.Split("Order", StringSplitOptions.RemoveEmptyEntries);
                                                xmlArtikel artikel = new xmlArtikel();

                                                //Voeg voor elke Order in xmlOrder.artikel
                                                foreach (string EachOrderString in EachOrder)
                                                {
                                                    if (!uniqueArtikel.Contains(EachOrderString) && !string.IsNullOrEmpty(EachOrderString))
                                                    {
                                                        uniqueArtikel.Add(EachOrderString);
                                                        var newDefObject = new defObject(); // Maak een nieuwe instantie van defObject

                                                        newDefObject.OrderTags = defObject.OrderTags; //Voeg een tagnaam toe voor later

                                                        newDefObject.Value = EachOrderString; // Wijs de waarde toe aan de nieuwe instantie
                                                        artikel.Artikelen.Add(newDefObject);

                                                    }
                                                    // bepaal of het een nieuw artikel moet worden of niet. j is dan de lijnnummer, hij wordt elke keer door de loop +10.
                                                    j += 10;
                                                }
                                                xmlOrder.Artikelen.Add(artikel);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (def == null)
                        {
                            memoryStream.Close();
                            fileStream.Close();

                            MoveOrDeleteFailedFile(fileName);
                        }
                        else
                        {
                            //overzetten als XML-bestand
                            var xmlFileExtension = System.IO.Path.ChangeExtension(fileName, ".xml");

                            if (Directory.Exists(MyConfig.FilePath))
                            {
                                destinationPath = MyConfig.FilePath;
                            }
                            else
                            {
                                destinationPath = defaultPath;
                            }

                            string xmlFilePath = destinationPath + "\\" + xmlFileExtension; // Het pad naar het XML-bestand

                            // Schrijf de geëxtraheerde tekst naar een XML-bestand
                            using (XmlWriter writer = XmlWriter.Create(xmlFilePath))
                            {
                                writer.WriteStartDocument();

                                writer.WriteStartElement("Order");

                                //Huidige PDF text
                                //Element dat in het configuratiebestand te vinden is
                                writer.WriteStartElement("Huidige_PDFtext");
                                writer.WriteString(text.ToString());
                                writer.WriteEndElement();
                                //
                                writer.WriteStartElement("stamgegevens");
                                xmlOrder.Items.ForEach(o => WriteXmlTag(writer, o));
                                writer.WriteEndElement();
                                //def.GetObjectsFromNiveau(XmlNiveau.Order).ForEach(o => WriteXmlTag(writer, o));

                                writer.WriteStartElement("Artikelen");

                                xmlOrder.Artikelen.ForEach(a =>
                                {
                                    a.Artikelen.ForEach(o => WriteXmlTagArtikel(writer, o));   
                                });

                                writer.WriteEndElement();

                                writer.WriteEndElement();
                                writer.WriteEndDocument();
                            }

                            memoryStream.Close();
                            fileStream.Close();

                            MoveOrDeletelSucceedFile(fileName);

                        }
                    }
                }
            }
            else if(fileExtension.ToLower() != ".xml")
            {
                MoveOrDeleteFailedFile(fileName);
            }
        }

        private void WriteXmlTag(XmlWriter writer, defObject o)
        {
            if (o.Value != null)
            {
                o.ToString().Split(" ");
                writer.WriteStartElement(o.TagNaam);
                writer.WriteString(o.Value); // Schrijf de geëxtraheerde tekst
                writer.WriteEndElement();
            }
        }

        public void WriteXmlTagArtikel(XmlWriter writer, defObject o)
        {
            if (o.Value != null)
            {
                writer.WriteStartElement("Artikel");
                string orderLine = o.Value.Replace("\n", " ");
                
                int i = 0;

                var referentieIndex = orderLine.Split(" ")[1];

                string[] item = new string[o.OrderTags.Count()];

                item[0] = orderLine.Split(" ")[0];

                item[o.OrderTags.Count() - 5] = orderLine.Split(" ").Reverse().Take(5).Last();
                item[o.OrderTags.Count() - 4] = orderLine.Split(" ").Reverse().Take(4).Last();
                item[o.OrderTags.Count() - 3] = orderLine.Split(" ").Reverse().Take(3).Last();
                item[o.OrderTags.Count() - 2] = orderLine.Split(" ").Reverse().Take(2).Last();
                item[o.OrderTags.Count() - 1] = orderLine.Split(" ").Last().Trim();

                foreach (var Ordertagnaam in o.OrderTags)
                {
                    string tagNaam = Ordertagnaam.TagNaam;
                    writer.WriteStartElement(tagNaam);

                    if (Regex.IsMatch(referentieIndex, @"\d"))
                    {
                        //Zo ja, dan is die de referentie
                        item[1] = referentieIndex;
                        item[2] = orderLine.Split(" ")[2];

                    }
                    else
                    {
                        int referentieFrom = orderLine.IndexOf(" ");
                        int referentieTo = orderLine.IndexOf(" ", referentieFrom + 1); // Zoek vanaf het karakter na het eerste spatie-teken
                        int derdeSpaceIndex = orderLine.IndexOf(" ", referentieTo + 1); // Zoek vanaf het karakter na de tweede spatie-teken

                        item[1] = orderLine.Substring(referentieFrom + 1, derdeSpaceIndex - referentieFrom - 1).Trim();
                        item[2] = orderLine.Split(" ")[3];

                    }
                    int materiaalOmschrijvingFrom = orderLine.IndexOf(item[2]) + item[2].Length;
                    int materiaalOmschrijvingTo = orderLine.IndexOf(item[o.OrderTags.Count() - 5]);

                    item[3] = orderLine.Substring(materiaalOmschrijvingFrom, materiaalOmschrijvingTo - materiaalOmschrijvingFrom).Trim();

                    writer.WriteString(item[i]);
                    writer.WriteEndElement();
                    i++;

                }
                writer.WriteEndElement();
            }
        }

        public void MoveOrDeleteFailedFile(string fileName)
        {
            string folderNaam = "Afgewezen";
            string filePath = MyConfig.FilePath;

            if (!Directory.Exists(filePath + "\\" + folderNaam))
            {
                Directory.CreateDirectory(filePath + "\\" + folderNaam);
            }
            string failedSourcePath = filePath + "\\" + fileName;
            if (!File.Exists(filePath + "\\" + folderNaam + "\\" + fileName))
            {
                File.Move(failedSourcePath, filePath + "\\" + folderNaam + "\\" + fileName);
            }
            else
            {
                File.Delete(filePath + "\\" + fileName);
            }
        }

        public void MoveOrDeletelSucceedFile(string fileName)
        {
            string foldernaam = "Verwerkt";
            string filePath = MyConfig.FilePath;

            if (!Directory.Exists(filePath + "\\" + foldernaam))
            {
                Directory.CreateDirectory(filePath + "\\" + foldernaam);
            }
            string succeedSourcePath = filePath + "\\" + fileName;
            if (!File.Exists(filePath + "\\" + foldernaam + "\\" + fileName))
            {
                File.Move(succeedSourcePath, filePath + "\\" + foldernaam + "\\" + fileName);
            }
            else
            {
                File.Delete(filePath + "\\" + fileName);
            }
        }
    }
}
