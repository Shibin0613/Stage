﻿using iTextSharp.text.pdf.parser;
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

namespace TransferTool
{
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

        internal string GetValue(PdfReader reader, string currentText, int i, defObject defObject, int j)
        {
            string value = string.Empty;
            //Als de huidige text geen deObject.Text.From bevat, dan overslaan. Dat heeft te maken dat er de pdf-pagina apart wordt uirgelezen,
            //en wat bij pagina 1 te vinden is, vind je niet per se ook in pagina2
            if (currentText.Contains(defObject.Text.From))
            {
                //Als defobject.Text.From en To niet leeg zijn, dan pak hij de From en to
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

                    //Als de pdf pagina minder dan 2 is. Dit is voor de orders.
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
                        if (currentText.Contains(y))
                        {
                            value = currentText.Substring(textFrom, textTo - textFrom).Replace("\n", "").Trim();
                        }
                        else 
                        {
                            value = currentText.Substring(textFrom).Trim();
                        }
                    }
                }
                //Anders pakt hij de coordinaten variabelen
                else
                {
                    float x = GetPostScriptPoints((float)defObject.Position.X);
                    float xy = GetPostScriptPoints((float)defObject.Position.Z);
                    float yx = GetPostScriptPoints((float)defObject.Position.W);

                    iTextSharp.text.Rectangle mediabox = reader.GetPageSize(i);

                    float y = mediabox.Height - GetPostScriptPoints((float)defObject.Position.Y);

                    //Value op position
                    System.util.RectangleJ rect = new System.util.RectangleJ(x, y, xy, yx);
                    RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
                    ITextExtractionStrategy strategy = new FilteredTextRenderListener(
                        new LocationTextExtractionStrategy(), filter);
                    value = PdfTextExtractor.GetTextFromPage(reader, i, strategy).Trim();
                }

                //defObject.OrderTags wordt van tevoren handmatig gedefinieerd. Als de defObject.Ordertags niet bestaat, sla deze stap over.
                if (defObject.OrderTags != null)
                {
                    StringBuilder order = new StringBuilder();

                    int orderFrom = currentText.IndexOf(defObject.Text.From);
                    //Dit geldt voor pagina 2, Omdat huidige text dan het Totaalbedrag bevat.
                    //Als orderFrom totaal bevat, dan betekent dat de artikel eindigt op pagina 1, anders gaat hij van pagina 2 ook verder


                    if (currentText.Contains(defObject.Text.To))
                        {
                        string orderWithText = currentText.Substring(orderFrom + defObject.Text.From.Length).Trim();
                        if (orderWithText.Contains(defObject.Text.To))
                        {
                            int orderTo = orderWithText.IndexOf(defObject.Text.To);
                            orderWithText = orderWithText.Substring(0, orderTo);

                            orderWithText = orderWithText.Replace(" \n", " ");
                            // Regex-patroon om elk artikel te matchen

                            string pattern = @"(.+?)\s *(.*?)(?:\n|$)";

                            // Productinfo voor elk besteld product....
                            foreach (Match match in Regex.Matches(orderWithText, pattern))
                            {
                                string lineNumber = match.Groups[1].Value; // Het lijnnummer
                                if (!lineNumber.EndsWith(" "))
                                {
                                    lineNumber = lineNumber + " ";
                                }
                                string orderLine = match.Groups[2].Value; // De bestellijn

                                if (lineNumber.StartsWith("0"))
                                {
                                    order.Append(" " + lineNumber + orderLine);
                                }
                                else if (lineNumber.StartsWith(j.ToString()))
                                {
                                    j += 10;
                                    order.Append("Order" + lineNumber + orderLine);
                                }
                                else
                                {
                                    if (char.IsLetter(lineNumber[0]) && !lineNumber.StartsWith(" "))
                                    {
                                        order.Append(" " + lineNumber + orderLine);
                                    }
                                    if (lineNumber.IndexOf('/') == 2 && !lineNumber.StartsWith(" "))
                                    {
                                        order.Append(" " + lineNumber + orderLine);
                                    }
                                    else
                                    {
                                        order.Append(lineNumber + orderLine);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int orderTo = orderWithText.IndexOf(defObject.Text.To);
                            string Order = orderWithText.Substring(0, orderTo - 0);
                            order.Append(Order);
                        }
                    }
                    //Anders pagina 1, van een bepaade text tot het einde.
                    else
                    {
                        string orderWithText = currentText.Substring(orderFrom + defObject.Text.From.Length).Trim();
                        if (orderWithText.Contains(defObject.Text.To))
                        {
                            int orderTo = orderWithText.IndexOf(defObject.Text.To);
                            string Order = orderWithText.Substring(0, orderTo - 0);
                            order.Append(Order);
                        }
                        else
                        {
                            orderWithText = orderWithText.Replace(" \n", " ");
                            // Regex-patroon om elk artikel te matchen

                            string pattern = @"(.+?)\s *(.*?)(?:\n|$)";

                            // Productinfo voor elk besteld product....
                            foreach (Match match in Regex.Matches(orderWithText, pattern))
                            {
                                string lineNumber = match.Groups[1].Value; // Het lijnnummer
                                if (!lineNumber.EndsWith(" "))
                                {
                                    lineNumber = lineNumber + " ";
                                }
                                string orderLine = match.Groups[2].Value; // De bestellijn

                                if (lineNumber.StartsWith("0"))
                                {
                                    order.Append(" " + lineNumber + orderLine);
                                }
                                else if (lineNumber.StartsWith(j.ToString()))
                                {
                                    j += 10;
                                    order.Append("Order" + lineNumber + orderLine);
                                }
                                else
                                {
                                    if (char.IsLetter(lineNumber[0]) && !lineNumber.StartsWith(" "))
                                    {
                                        order.Append(" " + lineNumber + orderLine);
                                    }
                                    else if (lineNumber.IndexOf('/') == 2 && !lineNumber.StartsWith(" "))
                                    {
                                        order.Append(" " + lineNumber + orderLine);
                                    }
                                    else
                                    {
                                        order.Append(lineNumber + orderLine);
                                    }
                                    
                                }
                            }
                        }
                    }
                    defObject.Value = order.ToString();
                    
                    return defObject.Value;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    defObject.Value = value;
                }
            }

            return value;
        }

        private float GetPostScriptPoints(float inch)
        {
            return inch * 72;
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
            def.IdentifierText.Add("Hotel");

            JsonHelpers.WriteToJsonFile(def, "../../../nhDef.json");*/

            //Bij het opstarten wordt alle configuratiebestand opgehaald
            ReadDefinitions();
        }
        
        //Hier wordt gekeken voor elke configuratiebestand of de geuploade bestand overeenkomt met de unieke tekst
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

            //*Loop door alle json.files in huidig map
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (var file in d.GetFiles("*.json"))
            { 
                var def = JsonHelpers.ReadFromJsonFile<pdfDefinition>(file.FullName);

                if (def.IdentifierText != null)
                {
                    pdfDefinitions.Add(def);
                }
            }
            //endloop
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
