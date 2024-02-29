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
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography.Xml;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PrototypeTransferTool
{
    public static class MyConfig
    {
        private static IConfiguration? _configuration;

        public static event EventHandler FilePathUpdated;

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
                    File.WriteAllText("appsettings.json", json);
                    // Update configuration after writing to appsettings.json
                    InitConfig(); // Reset configuration after updating appsettings.json
                    OnFilePathUpdated();
                }
                catch (Exception ex)
                {
                    // Foutafhandeling - log de uitzondering of neem andere maatregelen
                    Console.WriteLine($"Er is een fout opgetreden bij het bijwerken van appsettings.json: {ex.Message}");
                }
            }
        }

        private static void OnFilePathUpdated()
        {
            // Raise the event to inform subscribers
            FilePathUpdated?.Invoke(null, EventArgs.Empty);
        }

        public static string GetTextFromPDF
        {
            get
            { 
                return null;
            }
            set 
            {
                StringBuilder text = new StringBuilder();
                StringBuilder ordernummer = new StringBuilder();
                StringBuilder hotelnaam = new StringBuilder();
                StringBuilder aanvrager = new StringBuilder();
                StringBuilder afleveradres = new StringBuilder();
                StringBuilder leverancier = new StringBuilder();
                StringBuilder orderdatum = new StringBuilder();
                StringBuilder valuta = new StringBuilder();
                StringBuilder order = new StringBuilder();
                StringBuilder opmerking = new StringBuilder();
                StringBuilder totaal = new StringBuilder();

                StringBuilder betaalcondities = new StringBuilder();
                StringBuilder factuuradres = new StringBuilder();
                StringBuilder stuurfactuuraan = new StringBuilder();
                StringBuilder levering = new StringBuilder();

                using (PdfReader reader = new PdfReader(@"C:\Users\pansh\Desktop\Stage\TransferTool\test4.pdf"))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        //test position voor Order number(Neemt de hele string op inclusief de Inkoop order nummer .........)
                        /*System.util.RectangleJ rect = new System.util.RectangleJ(402f, 540f, 75f, 15f);
                        RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
                        ITextExtractionStrategy strategyForOrdernumber = new FilteredTextRenderListener(
                            new LocationTextExtractionStrategy(), filter);
                        string test = PdfTextExtractor.GetTextFromPage(reader, i, strategyForOrdernumber);*/

                        //Definieren alle text van PDF
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                        string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);

                        text.Append(currentText);

                        if (currentText.Contains("INKOOP ORDER NUMMER "))
                        {
                            //Ordernumber
                            int ordernummerFrom = currentText.IndexOf("INKOOP ORDER NUMMER ") + "INKOOP ORDER NUMMER ".Length;
                            int ordernummerTo = currentText.LastIndexOf("INKOOP ORDER DATUM EXCLUSIEF");
                            string orderNumber = currentText.Substring(ordernummerFrom, ordernummerTo - ordernummerFrom).Replace("\n", "");

                            ordernummer.Append(orderNumber);

                            //Hotelnaam op position
                            System.util.RectangleJ rectHotelnaam = new System.util.RectangleJ(524.16f, 538.68f, 254.88f, 6.84f);
                            RenderFilter[] filterHotelnaam = { new RegionTextRenderFilter(rectHotelnaam) };
                            ITextExtractionStrategy strategyHotelnaam = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterHotelnaam);
                            string hotelNaam = PdfTextExtractor.GetTextFromPage(reader, i, strategyHotelnaam).Trim();

                            hotelnaam.Append(hotelNaam);

                            //Aanvrager op positie
                            System.util.RectangleJ rectAanvrager = new System.util.RectangleJ(255f, 438f, 152f, 78f);
                            RenderFilter[] filterAanvrager = { new RegionTextRenderFilter(rectAanvrager) };
                            ITextExtractionStrategy strategyAanvrager = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterAanvrager);
                            string Aanvrager = PdfTextExtractor.GetTextFromPage(reader, i, strategyAanvrager);

                            aanvrager.Append(Aanvrager);

                            //Afleveradres op positie
                            System.util.RectangleJ rectAfleveradres = new System.util.RectangleJ(454.07f, 442.34f, 156.08f, 77.25f);
                            RenderFilter[] filterAfleveradres = { new RegionTextRenderFilter(rectAfleveradres) };
                            ITextExtractionStrategy strategyAfleveradres = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterAfleveradres);
                            string Afleveradres = PdfTextExtractor.GetTextFromPage(reader, i, strategyAfleveradres);

                            afleveradres.Append(Afleveradres);

                            //Leverancier op positie
                            System.util.RectangleJ rectLeverancier = new System.util.RectangleJ(622.25f, 410.88f, 158.40f, 108.71f);
                            RenderFilter[] filterLeverancier = { new RegionTextRenderFilter(rectLeverancier) };
                            ITextExtractionStrategy strategyLeverancier = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterLeverancier);
                            string Leverancier = PdfTextExtractor.GetTextFromPage(reader, i, strategyLeverancier);

                            leverancier.Append(Leverancier);

                            //Inkoopdatum op positie
                            System.util.RectangleJ rectInkoopdatum = new System.util.RectangleJ(232.29f, 377.24f, 85.88f, 22f);
                            RenderFilter[] filterInkoopdatum = { new RegionTextRenderFilter(rectInkoopdatum) };
                            ITextExtractionStrategy strategyInkoopdatum = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterInkoopdatum);
                            string Inkoopdatum = PdfTextExtractor.GetTextFromPage(reader, i, strategyInkoopdatum);

                            orderdatum.Append(Inkoopdatum);

                            //Valuta op positie
                            System.util.RectangleJ rectValuta = new System.util.RectangleJ(744.97f, 376.91f, 29.64f, 22f);
                            RenderFilter[] filterValuta = { new RegionTextRenderFilter(rectValuta) };
                            ITextExtractionStrategy strategyValuta = new FilteredTextRenderListener(
                                new LocationTextExtractionStrategy(), filterValuta);
                            string Valuta = PdfTextExtractor.GetTextFromPage(reader, i, strategyValuta);

                            valuta.Append(Valuta);

                            //Opmerking
                            if (currentText.Contains("OPMERKING :  "))
                            {

                                int opmerkingFrom = currentText.IndexOf("OPMERKING :  ") + "OPMERKING :  ".Length;
                                int opmerkingTo = currentText.LastIndexOf("LIJN I REFERENTIE");
                                string Opmerking = currentText.Substring(opmerkingFrom, opmerkingTo - opmerkingFrom).Replace("\n", "");

                                opmerking.Append(Opmerking);
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
                                string orderWithText = currentText.Substring(orderFrom + "AANTAL PRIJS EENHEID TOTAAL".Length).Trim();
                                if (orderWithText.Contains("TOTAAL"))
                                {
                                    int orderTo = orderWithText.IndexOf("TOTAAL");
                                    string Order = orderWithText.Substring(0, orderTo - 0);
                                    order.Append(Order);
                                }
                                else
                                {
                                    string[] eachOrderFrom = orderWithText.Split('\n');
                                    //Productinfo voor elk besteld product....
                                    foreach (string orderLine in eachOrderFrom)
                                    {
                                        order.AppendLine("Order"); // Voeg het lijnnummer toe
                                        order.AppendLine(orderLine); // Voeg de bestellijn toe
                                    }
                                }
                            }
                        }

                        if (currentText.Contains("BETAALCONDITIES"))
                        {
                            //Totaalbedrag
                            int totaalFrom = currentText.IndexOf("TOTAAL ") + "TOTAAL ".Length;
                            int totaalTo = currentText.IndexOf(" EUR");
                            string Totaal = currentText.Substring(totaalFrom, totaalTo - totaalFrom).Replace("\n", "");

                            totaal.Append(Totaal);

                            //Betaalcondities
                            int betaalFrom = currentText.IndexOf("BETAALCONDITIES") + "BETAALCONDITIES".Length;
                            int betaalTo = currentText.LastIndexOf("FACTUURADRES");
                            string betaalConditie = currentText.Substring(betaalFrom, betaalTo - betaalFrom).Trim();

                            betaalcondities.Append(betaalConditie);

                            //FactuurAdres
                            int factuurAdresFrom = currentText.IndexOf("FACTUURADRES") + "FACTUURADRES".Length;
                            int factuurAdresTo = currentText.LastIndexOf("STUUR FACTUUR AAN");
                            string factuurAdres = currentText.Substring(factuurAdresFrom, factuurAdresTo - factuurAdresFrom).Replace("\n", "");

                            factuuradres.Append(factuurAdres);

                            //Stuur factuur aan
                            int stuurFactuuraanFrom = currentText.IndexOf("STUUR FACTUUR AAN") + "STUUR FACTUUR AAN".Length;
                            int stuurFactuuraanTo = currentText.LastIndexOf("LEVERING");
                            string stuurFactuurAan = currentText.Substring(stuurFactuuraanFrom, stuurFactuuraanTo - stuurFactuuraanFrom).Replace("\n", "");

                            stuurfactuuraan.Append(stuurFactuurAan);

                            //Levering
                            int leveringFrom = currentText.IndexOf("LEVERING") + "LEVERING".Length;
                            int leveringTo = currentText.LastIndexOf("TOTAAL ");
                            string Levering = currentText.Substring(leveringFrom, leveringTo - leveringFrom).Trim();

                            levering.Append(Levering);
                        }
                    }
                }

                string xmlFilePath = @"C:\Users\pansh\Desktop\output4.xml"; // Het pad naar het XML-bestand

                // Schrijf de geëxtraheerde tekst naar een XML-bestand
                using (XmlWriter writer = XmlWriter.Create(xmlFilePath))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Order");
                    //Huidige PDF text
                    // Element dat de ORDER bevat
                    writer.WriteStartElement("Huidige_PDFtext");
                    writer.WriteString(text.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteStartElement("stamgegevens");

                    writer.WriteStartElement("O_INKOOP_ORDER_NUMMER");
                    writer.WriteString(ordernummer.ToString()); // Schrijf de geëxtraheerde tekst
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_HOTELNAAM");
                    writer.WriteString(hotelnaam.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_AANVRAGER");
                    writer.WriteString(aanvrager.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_AFLEVERADRES");
                    writer.WriteString(afleveradres.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_LEVERANCIER");
                    writer.WriteString(leverancier.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_INKOOP_ORDER_DATUM");
                    writer.WriteString(orderdatum.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_VALUTA");
                    writer.WriteString(valuta.ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteStartElement("Artikelen");

                    writer.WriteStartElement("A_OPMERKING");
                    writer.WriteString(opmerking.ToString());
                    writer.WriteEndElement();

                    //In Orders elke order een nieuwe element 'Order' creeren
                    string[] eachOrderFrom = order.ToString().Split("Order", StringSplitOptions.RemoveEmptyEntries);

                    //Productinfo voor elk besteld product....
                    foreach (string orderLine in eachOrderFrom)
                    {
                        writer.WriteStartElement("Artikel");
                        //Lijncode
                        int lijnFrom = orderLine.IndexOf(' ') + " ".Length;
                        int lijnTo = orderLine.IndexOf(' ');
                        string Lijn = orderLine.Substring(0, lijnTo).Trim();

                        writer.WriteStartElement("A_LIJN");
                        writer.WriteString(Lijn);
                        writer.WriteEndElement();

                        //Referentie
                        var referentieIndex = orderLine.Split(' ')[1];
                        string referentie = string.Empty;
                        string nhMateriaalId = string.Empty;
                        string materiaalOmschrijving = string.Empty;
                        string leverdatum = string.Empty;
                        string aantal = string.Empty;
                        string prijs = string.Empty;
                        string eenheid = string.Empty;
                        string totaalAantal = string.Empty;
                        //Check of de string na tweede spatie een cijfer bevat
                        if (Regex.IsMatch(referentieIndex, @"\d"))
                        {
                            //Zo ja, dan is die de referentie
                            referentie = referentieIndex;
                            nhMateriaalId = orderLine.Split(' ')[2];

                            leverdatum = orderLine.Split(' ').Reverse().Take(5).Last();
                            aantal = orderLine.Split(' ').Reverse().Take(4).Last();
                            prijs = orderLine.Split(' ').Reverse().Take(3).Last();
                            eenheid = orderLine.Split(' ').Reverse().Take(2).Last();
                            totaalAantal = orderLine.Split(" ").Last().Trim();
                            //Materiaal omschrijving tussen NH MATERIAAL ID en LEVERDATUM
                            //Materiaal omschrijving tussen NH MATERIAAL ID en LEVERDATUM
                            int materiaalOmschrijvingFrom = orderLine.IndexOf(nhMateriaalId) + nhMateriaalId.Length;
                            int materiaalOmschrijvingTo = orderLine.IndexOf(leverdatum);
                            materiaalOmschrijving = orderLine.Substring(materiaalOmschrijvingFrom, materiaalOmschrijvingTo - materiaalOmschrijvingFrom).Trim();
                        }
                        else
                        {
                            //zo niet, dan komt voor de cijfers nog een toevoeging
                            int referentieFrom = orderLine.IndexOf(" ");
                            int referentieTo = orderLine.IndexOf(" ", referentieFrom + 1); // Zoek vanaf het karakter na het eerste spatie-teken
                            int derdeSpaceIndex = orderLine.IndexOf(" ", referentieTo + 1); // Zoek vanaf het karakter na de tweede spatie-teken

                            referentie = orderLine.Substring(referentieFrom + 1, derdeSpaceIndex - referentieFrom - 1).Trim();
                            nhMateriaalId = orderLine.Split(' ')[3];

                            leverdatum = orderLine.Split(' ').Reverse().Take(5).Last();
                            aantal = orderLine.Split(' ').Reverse().Take(4).Last();
                            prijs = orderLine.Split(' ').Reverse().Take(3).Last();
                            eenheid = orderLine.Split(' ').Reverse().Take(2).Last();
                            totaalAantal = orderLine.Split(" ").Last().Trim();
                            //Materiaal omschrijving tussen NH MATERIAAL ID en LEVERDATUM
                            int materiaalOmschrijvingFrom = orderLine.IndexOf(nhMateriaalId) + nhMateriaalId.Length;
                            int materiaalOmschrijvingTo = orderLine.IndexOf(leverdatum);
                            materiaalOmschrijving = orderLine.Substring(materiaalOmschrijvingFrom, materiaalOmschrijvingTo - materiaalOmschrijvingFrom).Trim();
                        }
                        writer.WriteStartElement("A_REFERENTIE");
                        writer.WriteString(referentie);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_NH_MATERIAAL_ID");
                        writer.WriteString(nhMateriaalId);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_MATERIAAL_OMSCHRIJVING");
                        writer.WriteString(materiaalOmschrijving);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_LEVERDATUM");
                        writer.WriteString(leverdatum);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_AANTAL");
                        writer.WriteString(aantal);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_PRIJS");
                        writer.WriteString(prijs);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_EENHEID");
                        writer.WriteString(eenheid);
                        writer.WriteEndElement();

                        writer.WriteStartElement("A_TOTAAL");
                        writer.WriteString(totaalAantal);
                        writer.WriteEndElement();


                        writer.WriteString(orderLine.Trim());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_TOTAAL");
                    writer.WriteString(totaal.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("factuur");

                    writer.WriteStartElement("O_BETAALCONDITIES");
                    writer.WriteString(betaalcondities.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_FACTUURADRES");
                    writer.WriteString(factuuradres.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_STUUR_FACTUUR_AAN");
                    writer.WriteString(stuurfactuuraan.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("O_LEVERING");
                    writer.WriteString(levering.ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                // Toon een berichtvenster dat het XML-bestand is gemaakt
                MessageBox.Show("XML-bestand is gemaakt op: " + xmlFilePath);
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
