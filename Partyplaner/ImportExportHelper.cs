using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Partyplaner
{
    public class ImportExportHelper
    {
        private const string XML_PATH = "config.xml";
        public Dictionary<string, Gast> gaesteliste = new Dictionary<string, Gast>();
        private static ImportExportHelper importExportHelper;
        private Tuple<int,int> tischGroesse { get; set; }
        private Tuple<int,int> tischPosition { get; set; }
        private Tuple<int, int> raumGroesse { get; set; }
        private int iterationen { get; set; }

        private static bool initializied = false;

		/// <summary>
		/// Der Konstruktor der Klasse.
		/// </summary>
		private ImportExportHelper()
        {

            Konfig_Form konfigForm = new Konfig_Form();
            konfigForm.FormClosed += loadXMLValues;
            konfigForm.ShowDialog();
        }

        /// <summary>
        /// Liefert die Instanz der Klasse.
        /// </summary>
        /// <returns>Die Instanz der Klasse.</returns>
        public static ImportExportHelper getImportExportHelper()
        {
            if (importExportHelper == null)
            {
                importExportHelper = new ImportExportHelper();
                while(!initializied)
                {

                }
            }
            return importExportHelper;
        }

        /// <summary>
        /// Liest die relevanten Daten aus der XML Datei.
        /// </summary>
        private void loadXMLValues(object sender, EventArgs e)
        {
            // Extrahieren der Daten
            XElement configRoot = null;
            try
            {
                configRoot = XElement.Load(XML_PATH);
            }
            catch (FileNotFoundException ex)
            {
                // Noch keine Konfiguration vorhanden
                return;
            }
            var gaestelisteNode = configRoot.Element("gaesteliste");
            var raumNode = configRoot.Element("raum");
            var raumGroesseNode = raumNode.Element("groesse");
            var raumTischNode = raumNode.Element("tisch");
            var raumTischGroesseNode = raumTischNode.Element("groesse");
            var raumTischPositionNode = raumTischNode.Element("position");

            foreach (var gastNode in gaestelisteNode.Elements("gast"))
            {
                var name = gastNode.Attribute("name").Value;
                var beruf = gastNode.Attribute("beruf").Value;
                var position = ExtractXYDataFromNode(gastNode.Element("startposition"));
                var wunschabstaende = new Dictionary<string, int>();
                var wunschabstaendeNodes = gastNode.Element("wunschabstaende").Elements("wunschabstand");
                foreach (var eachWunschabstandNode in wunschabstaendeNodes)
                {
                    var abstandName = eachWunschabstandNode.Attribute("gast").Value;
                    var abstandDistanz = Convert.ToInt32(eachWunschabstandNode.Attribute("distanz").Value);
                    wunschabstaende.Add(abstandName, abstandDistanz);
                }
                // Hier könnte man noch eine If-Prüfung einbauen ob der Gast schon existiert.
                gaesteliste.Add(name, new Gast(name, beruf, new System.Drawing.Point(position.Item1, position.Item2), wunschabstaende));
            }

            raumGroesse = ExtractXYDataFromNode(raumGroesseNode);
            tischGroesse = ExtractXYDataFromNode(raumTischGroesseNode);
            tischPosition = ExtractXYDataFromNode(raumTischPositionNode);

            initializied = true;
        }

        /// <summary>
		/// Speichert den Inhalt der Oberflächenfelder in die XML-Datei.
		/// </summary>
		private void SaveUiToXml()
		{
			var configRoot = new XElement("konfiguration");
			// Gästeliste erzeugen
			var gaestelisteNode = new XElement("gaesteliste");
			configRoot.Add(gaestelisteNode);

			foreach (Gast eachGast in gaesteliste)
			{
				gaestelisteNode.Add(CreateXmlNodeForGast(eachGast));
			}
			// Raum erzeugen
			var raumNode = new XElement("raum");
			configRoot.Add(raumNode);
			raumNode.Add(CreateXYNode("groesse", raumGroesse.Item1, raumGroesse.Item2));

			var tischNode = new XElement("tisch");
			tischNode.Add(CreateXYNode("groesse", tischGroesse.Item1, tischGroesse.Item2));
			tischNode.Add(CreateXYNode("position", tischPosition.Item1, tischPosition.Item2));
			raumNode.Add(tischNode);

			configRoot.Save(XML_PATH);
            Close();
		}

        public void setTischPosition(int x, int y) 
        {
            tischPosition = new Tuple<int, int>(x,y);
        }

        public void setTischGroesse(int x, int y) 
        {
            tischGroesse = new Tuple<int, int>(x,y);
        }

        public void setRaumGroesse(int x, int y) 
        {
            raumGroesse = new Tuple<int, int>(x,y);
        }

        public void setGaesteliste(Dictionary<string, Gast> gaesteListe)
        {
            gaesteliste = gaesteListe;
        }
        
        /// <summary>
		/// Erzeugt eine XML-Node mit X-Y-Attributen.
		/// </summary>
		/// <param name="name">Der Name der Node.</param>
		/// <param name="x">Der X-Wert der Node.</param>
		/// <param name="y">Der Y-Wert der Node.</param>
		/// <returns>Die erzeugte Node.</returns>
		private XElement CreateXYNode(string name, decimal x, decimal y)
		{
			var node = new XElement(name);
			node.SetAttributeValue("x", x);
			node.SetAttributeValue("y", y);
			return node;
		}

        /// <summary>
        /// Liefert für einen Gastnamen den dazugehörigen Gast aus der Gaesteliste.
        /// </summary>
        /// <param name="guestName"> Der Name des zu suchenden Gast.</param>
        /// <returns>Den gesuchten Gast.</returns>
        public Gast GetGast(string guestName)
        {
            foreach (Gast eachGast in gaesteliste.Values)
            {
                if(eachGast.name == guestName)
                {
                    return eachGast;
                }
            }
			return null;
        }

        public Dictionary<string, Gast> GetGaesteListe()
        {
            return gaesteliste;
        }

        public Tuple<int, int> GetTischGrosse()
        {
            return tischGroesse;
        }

        public Tuple<int, int> GetTischPosition()
        {
            return tischPosition;
        }

        public Tuple<int,int> GetRaum()
        {
            return raumGroesse;
        }

        public int GetAnzahlGaeste()
        {
            return gaesteliste.Count;
        }

        public System.Drawing.Point GetAktuellePosition(string name)
        {
            return GetGast(name).position;
        }

        public List<int> GetBefindlichkeiten()
        {
            List<int> befindlichkeiten = new List<int>();
            foreach (Gast eachGast in gaesteliste.Values)
            {
                befindlichkeiten.Add(eachGast.befindlichkeit);
            }
            return befindlichkeiten;
        }

        /// <summary>
        /// Liefert die XY-Attribute einer XY-Node als Tupel zurück.
        /// </summary>
        /// <param name="node">Die Node, deren XY-Attribute extrahiert werden sollen.</param>
        /// <returns>Die XY-Attribute.</returns>
        private Tuple<int, int> ExtractXYDataFromNode(XElement node)
        {
            var x = Convert.ToInt32(node.Attribute("x").Value);
            var y = Convert.ToInt32(node.Attribute("y").Value);
            return new Tuple<int, int>(x, y);
        }
    }
}