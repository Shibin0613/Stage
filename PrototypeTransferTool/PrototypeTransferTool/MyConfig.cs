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
using Org.BouncyCastle.Asn1.X509;

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
                StringBuilder order = new StringBuilder();
                using (PdfReader reader = new PdfReader(@"C:\Users\pansh\Desktop\Stage\TransferTool\test1.pdf"))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        //test position voor Order number
                        System.util.RectangleJ rect = new System.util.RectangleJ(402f, 540f, 75f, 15f);
                        RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
                        ITextExtractionStrategy strategyForOrdernumber = new FilteredTextRenderListener(
                            new LocationTextExtractionStrategy(), filter);
                        string test = PdfTextExtractor.GetTextFromPage(reader, i, strategyForOrdernumber);

                        //Definieren alle text van PDF
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                        string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);

                        if (currentText.Contains("INKOOP ORDER NUMMER "))
                        {
                            //Ordernumber
                            int ordernumberFrom = currentText.IndexOf("INKOOP ORDER NUMMER ") + "INKOOP ORDER NUMMER ".Length;
                            int ordernumberTo = currentText.LastIndexOf("INKOOP ORDER DATUM EXCLUSIEF");
                            string orderNumber = currentText.Substring(ordernumberFrom, ordernumberTo - ordernumberFrom).Replace("\n", "");

                            //Inkoopdatum
                            int inkoopDatumFrom = currentText.IndexOf("el servicio.") + "el servicio.".Length;
                            int inkoopDatumTo = currentText.IndexOf("INKOOP ORDER NUMMER ");
                            string inkoopDatum = currentText.Substring(inkoopDatumFrom, inkoopDatumTo - inkoopDatumFrom).Replace("\n", "");

                            //Hotelnaam op position
                            System.util.RectangleJ rectHotelnaam = new System.util.RectangleJ(524.16f, 538.68f, 254.88f, 6.84f);
                            RenderFilter[] filterHotelnaam = { new RegionTextRenderFilter(rectHotelnaam) };
                            ITextExtractionStrategy strategyHotelnaam = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterHotelnaam);
                            string hotelNaam = PdfTextExtractor.GetTextFromPage(reader, i, strategyHotelnaam);

                            //Aanvrager
                            System.util.RectangleJ rectAanvrager = new System.util.RectangleJ(255f, 438f, 152f, 78f);
                            RenderFilter[] filterAanvrager = { new RegionTextRenderFilter(rectAanvrager) };
                            ITextExtractionStrategy strategyAanvrager = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterAanvrager);
                            string Aanvrager = PdfTextExtractor.GetTextFromPage(reader, i, strategyAanvrager);

                            
                            //afleveradres
                            int afleverAdresFrom = currentText.IndexOf("OPMERKING :  ") + "OPMERKING :  ".Length;
                            int afleverAdresTo = currentText.LastIndexOf("LIJN I REFERENTIE");
                            string afleverAdres = currentText.Substring(afleverAdresFrom, afleverAdresTo - afleverAdresFrom);


                            if (currentText.Contains("OPMERKING :  "))
                            {
                                //Opmerking
                                int opmerkingFrom = currentText.IndexOf("NEDERLAND") + "NEDERLAND".Length;
                                int opmerkingTo = currentText.LastIndexOf(hotelNaam);
                                string Opmerking = currentText.Substring(opmerkingFrom, opmerkingTo - opmerkingFrom);
                            }

                            //Order
                            int orderFrom = currentText.IndexOf("AANTAL PRIJS EENHEID TOTAAL");
                            string page2 = "Página";
                            if (currentText.Contains(page2))
                            {
                                int orderTo = currentText.LastIndexOf(page2);
                                string Order = currentText.Substring(0, orderTo - 0);
                                order.Append(Order);
                            }
                            else
                            {
                                string orderWithText = currentText.Substring(orderFrom + "AANTAL PRIJS EENHEID TOTAAL".Length);
                                if (orderWithText.Contains("TOTAAL"))
                                {
                                    int orderTo = orderWithText.IndexOf("TOTAAL");
                                    string Order = orderWithText.Substring(0, orderTo - 0);
                                    order.Append(Order);
                                }
                                else
                                {
                                    order.Append(orderWithText);
                                }
                            }
                        }

                        if (currentText.Contains("BETAALCONDITIES"))
                        {
                            //Betaalcondities
                            int betaalFrom = currentText.IndexOf("BETAALCONDITIES") + "BETAALCONDITIES".Length;
                            int betaalTo = currentText.LastIndexOf("FACTUURADRES");
                            string betaalConditie = currentText.Substring(betaalFrom, betaalTo - betaalFrom);

                            //FactuurAdres
                            int factuurAdresFrom = currentText.IndexOf("FACTUURADRES") + "FACTUURADRES".Length;
                            int factuurAdresTo = currentText.LastIndexOf("STUUR FACTUUR AAN");
                            string factuurAdres = currentText.Substring(factuurAdresFrom, factuurAdresTo - factuurAdresFrom);

                            //Stuur factuur aan
                            int stuurFactuuraanFrom = currentText.IndexOf("STUUR FACTUUR AAN") + "STUUR FACTUUR AAN".Length;
                            int stuurFactuuraanTo = currentText.LastIndexOf("LEVERING");
                            string stuurFactuurAan = currentText.Substring(stuurFactuuraanFrom, stuurFactuuraanTo - stuurFactuuraanFrom);

                            //Levering
                            int leveringFrom = currentText.IndexOf("LEVERING") + "LEVERING".Length;
                            int leveringTo = currentText.LastIndexOf("TOTAAL ");
                            string Levering = currentText.Substring(leveringFrom, leveringTo - leveringFrom);
                        }

                        text.Append(currentText);
                    }
                }

                string xmlFilePath = @"C:\Users\pansh\Desktop\output1.xml"; // Het pad naar het XML-bestand

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
