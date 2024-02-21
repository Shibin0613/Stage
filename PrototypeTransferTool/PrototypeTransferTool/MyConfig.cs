using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace PrototypeTransferTool
{
    public static class MyConfig
    {
        private static IConfiguration? _configuration;

        public static void InitConfig()
        {
            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        }

        public static string FilePath 
        {
            get
            {
                var path = _configuration?.GetValue<string>("FileStorage");
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = "c:\\temp";
                }
                return path;
            }
            set
            {
                var json = JsonSerializer.Serialize(new { FileStorage = value });
                try
                {
                    File.WriteAllText("../../../appsettings.json", json);
                    // Update configuration after writing to appsettings.json
                    InitConfig(); // Reset configuration after updating appsettings.json
                }
                catch (Exception ex)
                {
                    // Foutafhandeling - log de uitzondering of neem andere maatregelen
                    Console.WriteLine($"Er is een fout opgetreden bij het bijwerken van appsettings.json: {ex.Message}");
                }
            }
        }

        public static string GetTextFromPDF
        {
            get
            {
                StringBuilder text = new StringBuilder();
                using (PdfReader reader = new PdfReader(@"C:\Users\pansh\Desktop\test.pdf"))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy).Replace("\n", string.Empty);
                        int orderNumberFrom = currentText.IndexOf("INKOOP ORDER NUMMER ") + "INKOOP ORDER NUMMER ".Length;
                        int orderNumberTo = currentText.LastIndexOf("INKOOP ORDER DATUM");
                        string orderNumber = currentText.Substring(orderNumberFrom, orderNumberTo - orderNumberFrom);

                        int afleveradresFrom = currentText.IndexOf("AFLEVERADRES") + "AFLEVERADRES".Length;
                        int afleveradresTo = currentText.LastIndexOf("RUBEN VAN WIJK");
                        string aanvrager = currentText.Substring(afleveradresFrom, afleveradresTo - afleveradresFrom);

                        text.Append(currentText); // Voeg de geëxtraheerde tekst toe aan de StringBuilder
                    }
                }

                string xmlFilePath = @"C:\Users\pansh\Desktop\output.xml"; // Het pad naar het XML-bestand

                // Schrijf de geëxtraheerde tekst naar een XML-bestand
                using (XmlWriter writer = XmlWriter.Create(xmlFilePath))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("PDFText"); // Element dat de PDF-tekst bevat
                    writer.WriteString(text.ToString()); // Schrijf de geëxtraheerde tekst
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                // Toon een berichtvenster dat het XML-bestand is gemaakt
                MessageBox.Show("XML-bestand is gemaakt op: " + xmlFilePath);

                return text.ToString();
            }
        }

        public static string? GetDirectory(string startDir)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.InitialDirectory = startDir;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }
            return null;
        }
    }
}
