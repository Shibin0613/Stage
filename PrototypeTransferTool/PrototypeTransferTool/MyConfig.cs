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
using Microsoft.Web.WebView2.Core;
using System.Web;
using BitMiracle.LibTiff.Classic;

namespace PrototypeTransferTool
{
    //public enum ProdistFieldType
    //{
    //    Klantnummer, Klantnaam, Ordernummer, Orderomschrijving, Artikelcode, Artikelaantal, Artikelomschrijving, Referentie
    //}

    public enum XmlNiveau
    {
        Order, Artikel
    }

    public class defPosition
    {
        public double X { get; set; }    
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

    }
    public class defText
    {
        public string From { get; set; }    
        public string To { get; set; }
    }

    public class defOrder
    {
        public string TagNaam { get; set; }
        public string Value { get; set; }
    }
        public class defObject
    {
        //public ProdistFieldType Type { get; set; }
        public defPosition Position { get; set; }
        public defText Text { get; set; }
        public string TagNaam { get; set; }

        public XmlNiveau XmlNiveau{ get; set; }
        public string Value { get; set; }

        public List<defOrder> OrderTags { get; set; }



        internal string GetValue(PdfReader reader, string currentText, int i, defObject defObject)
        {
            string value = string.Empty;
            if (currentText.Contains(defObject.Text.From))
            {
                if (!string.IsNullOrWhiteSpace(defObject.Text.From) || !string.IsNullOrWhiteSpace(defObject.Text.To))
                {
                    string x = defObject.Text.From.ToString();
                    string y = defObject.Text.To.ToString();

                    int textFrom = currentText.IndexOf(x) + x.Length;
                    int textTo = currentText.IndexOf(y);

                    if (textTo < textFrom)
                    {
                        textTo = currentText.LastIndexOf(y);
                    }

                    if (i < 2)
                    {
                        value = currentText.Substring(textFrom).Trim();
                        if (!value.Contains(y))
                        {
                            value = currentText.Substring(textFrom).Trim();
                        }
                        else 
                        {
                            value = currentText.Substring(textFrom, textTo - textFrom).Replace("\n", "").Trim();
                        }
                    }
                    else
                    {
                        value = currentText.Substring(textFrom, textTo - textFrom).Replace("\n", "").Trim();
                    }
                }
                else
                {
                    float x = (float)defObject.Position.X;
                    float y = (float)defObject.Position.Y;
                    float xy = (float)defObject.Position.Z;
                    float yx = (float)defObject.Position.W;

                    //Hotelnaam op position
                    System.util.RectangleJ rect = new System.util.RectangleJ(x, y, xy, yx);
                    RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
                    ITextExtractionStrategy strategy = new FilteredTextRenderListener(
                        new LocationTextExtractionStrategy(), filter);
                    value = PdfTextExtractor.GetTextFromPage(reader, i, strategy).Trim();
                }
                if (defObject.OrderTags != null)
                {
                    int z = 0;
                    string[] items = value.Split(" ");

                    var Aantal = items.Reverse().Take(4).Last();
                    var prijsPerStuk = items.Reverse().Take(3).Last();
                    var Iets = items.Reverse().Take(2).Last();
                    var Totaal = items.Last();
                    
                    for (int j = 0; j < Math.Min(items.Length, defObject.OrderTags.Count); j++)
                    {
                        if (Regex.IsMatch(items[j], @"\d"))
                        {
                            defObject.OrderTags[z].Value = items[j];
                            z++;
                        }
                        else 
                        {
                            //zo niet, dan komt voor de cijfers nog een toevoeging
                            if (j < items.Length - 1) // Zorg ervoor dat er een volgend item is om samen te voegen
                            {
                                defObject.OrderTags[j].Value = $"{items[j]} {items[j + 1]}";
                                z++; // Sla het volgende item over omdat het al is samengevoegd
                                j++;
                            }
                            else
                            {
                                // Als er geen volgend item is, behoud het huidige item
                                defObject.OrderTags[j].Value = items[j];
                            }

                        }
                    }

                }

                if (!string.IsNullOrEmpty(value))
                {
                    defObject.Value = value;
                }
            }

            return value;
        }
    }

    public class xmlOrder
    {
        public List<defObject> Items { get; set; }
        public List<xmlArtikel> Artikelen { get; set; }
        public xmlOrder()
        {
            Items = new List<defObject>();
            Artikelen = new List<xmlArtikel>();
        }
    }
    public class xmlArtikel
    {
        public List<defObject> Artikelen { get; set; }

        public xmlArtikel()
        {
            Artikelen = new List<defObject>();
        }
    }

    public class pdfDefinition
    {
        public List<defObject> defObjects { get; set; }
        public List<string> IdentifierText { get; set; }

        public List<defObject>? GetObjectsFromNiveau(XmlNiveau niveau)
        {
            return defObjects.Where(o => o.XmlNiveau == niveau).ToList();
        }
    }

    public static class MyConfig
    {
        private static IConfiguration? _configuration;

        public static event EventHandler FilePathUpdated;

        public static List<pdfDefinition> pdfDefinitions { get; set; }

        public static void InitConfig()
        {
            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            /*var def = new pdfDefinition();*/
            /*def.defObjects = new List<defObject>();

            var defObj = new defObject();
            defObj.TagNaam = "KLANTNUMMER";
            defObj.XmlNiveau = XmlNiveau.Order;
            defObj.Text = new defText { From = "bla", To = "bla2" };
            defObj.Position = new defPosition { X = 1.1, Y = 1.2, W = 1.3, Z = 1.4 };

            def.defObjects.Add(defObj);

            defObj = new defObject();
            defObj.TagNaam = "ORDERNUMMER";
            defObj.XmlNiveau = XmlNiveau.Order;
            defObj.Text = new defText { From = "bla", To = "bla2" };
            defObj.Position = new defPosition { X = 1.1, Y = 1.2, W = 1.3, Z = 1.4 };

            def.defObjects.Add(defObj);

            def.IdentifierText = new List<string>();
            def.IdentifierText.Add("NH");
            def.IdentifierText.Add("Hotels");

            JsonHelpers.WriteToJsonFile(def, "../../../nhDef.json");*/
            ReadDefinitions();

            var def1 = JsonHelpers.ReadFromJsonFile<pdfDefinition>("../../../def_NH hotels.json");

            var s = def1.IdentifierText;
        }
        
        public static pdfDefinition? GetDefinition(string text)
        {
            
            return pdfDefinitions?.Find(pdfDefinition => text.ContainsText(pdfDefinition.IdentifierText));
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

        private static void ReadDefinitions()
        {
            pdfDefinitions = new List<pdfDefinition>();
            //*loop files
            var def = JsonHelpers.ReadFromJsonFile<pdfDefinition>("../../../def_NH hotels.json");

            pdfDefinitions.Add(def);
            //endloop
            
        }
        private static void WriteDefinitions()
        {
            pdfDefinitions.ForEach(pdfDefinition =>
            {
                var file = "../../../nhDef.json";
                JsonHelpers.WriteToJsonFile(pdfDefinition, file);
            });
        }

        private static void OnFilePathUpdated()
        {
            // Raise the event to inform subscribers
            FilePathUpdated?.Invoke(null, EventArgs.Empty);
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
