using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Partyplaner
{
	public partial class Konfig_Form : Form
    {
		// TODO Konfiguration: Pfad dynamisch machen
		private const string XML_PATH = "config.xml";
		private BindingList<Gast> gaesteliste = new BindingList<Gast>();

		public Konfig_Form()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Initialisiert die Form.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="e">Eventparameter</param>
		private void Form1_Load(object sender, EventArgs e)
        {
			FillUiFromXml();
			list_gaesteliste.DataSource = gaesteliste;
			list_gaesteliste.DisplayMember = "name";
			list_gaesteliste.SelectedValueChanged += OnGastSelectionChanged;
			txt_name.TextChanged += OnNameChanged;
			txt_beruf.TextChanged += OnBerufChanged;
			num_startpos_x.ValueChanged += OnStartpositionXChanged;
			num_startpos_y.ValueChanged += OnStartpositionYChanged;
			list_gaeste_wunschabstand.SelectedValueChanged += OnWunschabstandSelectionChanged;
			num_wunschabstand.ValueChanged += OnWunschabstandChanged;
			list_gaesteliste.ClearSelected();
		}

		/// <summary>
		/// Befüllt die Oberflächenfelder aus der XML-Datei.
		/// </summary>
		private void FillUiFromXml()
		{
			// Extrahieren der Daten
			XElement configRoot = null;
			try
			{
				configRoot = XElement.Load(XML_PATH);
			}
			catch (FileNotFoundException ex)
			{
                Console.WriteLine(ex);
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
				gaesteliste.Add(new Gast(name, beruf, new Point(position.Item1, position.Item2), wunschabstaende));
			}
			var raumGroesse = ExtractXYDataFromNode(raumGroesseNode);
			var tischGroesse = ExtractXYDataFromNode(raumTischGroesseNode);
			var tischPosition = ExtractXYDataFromNode(raumTischPositionNode);

			// Befüllen der Oberfläche
			num_raum_groesse_x.Value = raumGroesse.Item1;
			num_raum_groesse_y.Value = raumGroesse.Item2;
			num_tisch_groesse_x.Value = tischGroesse.Item1;
			num_tisch_groesse_y.Value = tischGroesse.Item2;
			num_tisch_pos_x.Value = tischPosition.Item1;
			num_tisch_pos_y.Value = tischPosition.Item2;
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

			foreach (Gast eachGast in list_gaesteliste.Items)
			{
				gaestelisteNode.Add(CreateXmlNodeForGast(eachGast));
			}
			// Raum erzeugen
			var raumNode = new XElement("raum");
			configRoot.Add(raumNode);
			raumNode.Add(CreateXYNode("groesse", num_raum_groesse_x.Value, num_raum_groesse_y.Value));

			var tischNode = new XElement("tisch");
			tischNode.Add(CreateXYNode("groesse", num_tisch_groesse_x.Value, num_tisch_groesse_y.Value));
			tischNode.Add(CreateXYNode("position", num_tisch_pos_x.Value, num_tisch_pos_y.Value));
			raumNode.Add(tischNode);

			configRoot.Save(XML_PATH);
            Close();
		}

		/// <summary>
		/// Erzeugt eine XML-Node für den übergebenen Gast.
		/// </summary>
		/// <param name="aGast">Der Gast, dessen XML-Node erzeugt werden soll.</param>
		/// <returns>Die XML-Node.</returns>
		private XElement CreateXmlNodeForGast(Gast aGast)
		{
			XElement gastNode = new XElement("gast");
			gastNode.SetAttributeValue("name", aGast.name);
			gastNode.SetAttributeValue("beruf", aGast.beruf);
			gastNode.Add(CreateXYNode("startposition", aGast.position.X, aGast.position.Y));
			XElement wunschabstaendeListNode = new XElement("wunschabstaende");
			gastNode.Add(wunschabstaendeListNode);

			foreach (Gast otherGast in list_gaesteliste.Items)
			{
				var wunschabstandNode = new XElement("wunschabstand");
				// TODO Konfiguration: Exception, wenn Wunschabstand fehlt?
				int wunschabstand = 0;
				if (aGast.wunschabstaende.ContainsKey(otherGast.name))
				{
					wunschabstand = aGast.wunschabstaende[otherGast.name];
				}
				wunschabstandNode.SetAttributeValue("gast", otherGast.name);
				wunschabstandNode.SetAttributeValue("distanz", wunschabstand);

				wunschabstaendeListNode.Add(wunschabstandNode);
			}
			return gastNode;
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
		/// Liefert den aktuell an der Oberfläche selektierten Gast zurück.
		/// </summary>
		/// <returns>Der selektierte Gast.</returns>
		private Gast GetSelectedGast()
		{
			return (Gast)list_gaesteliste.SelectedItem;
		}

		/// <summary>
		/// EventHandler bei Änderung der Gastselektion.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnGastSelectionChanged(object sender, EventArgs ev)
		{
			var selectedGast = GetSelectedGast();
			if (selectedGast != null)
			{
				txt_name.Enabled = true;
				txt_beruf.Enabled = true;
				num_startpos_x.Enabled = true;
				num_startpos_y.Enabled = true;
				btn_gast_loeschen.Enabled = true;


				txt_name.Text = selectedGast.name;
				txt_beruf.Text = selectedGast.beruf;
				num_startpos_x.Value = selectedGast.position.X;
				num_startpos_y.Value = selectedGast.position.Y;
				list_gaeste_wunschabstand.BeginUpdate();
				list_gaeste_wunschabstand.Items.Clear();
				foreach (Gast eachGast in gaesteliste)
				{
					if (eachGast == selectedGast)
					{
						continue;
					}
					list_gaeste_wunschabstand.Items.Add(eachGast);
				}
				list_gaeste_wunschabstand.EndUpdate();
				num_wunschabstand.Enabled = false;
			} else
			{
				list_gaeste_wunschabstand.BeginUpdate();
				list_gaeste_wunschabstand.Items.Clear();
				list_gaeste_wunschabstand.EndUpdate();
				txt_name.Text = "";
				txt_beruf.Text = "";
				num_startpos_x.Value = 0;
				num_startpos_y.Value = 0;
				num_wunschabstand.Value = 0;
				txt_name.Enabled = false;
				txt_beruf.Enabled = false;
				num_startpos_x.Enabled = false;
				num_startpos_y.Enabled = false;
				btn_gast_loeschen.Enabled = false;
				num_wunschabstand.Enabled = false;
			}

			
		}

		/// <summary>
		/// EventHandler bei Änderung des Gästenamens.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnNameChanged(object sender, EventArgs ev)
		{
			var newName = txt_name.Text;
			if (string.IsNullOrEmpty(newName))
			{
				return;
			}
			GetSelectedGast().name = newName;
		}

		/// <summary>
		/// EventHandler bei Änderung des Berufs.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnBerufChanged(object sender, EventArgs ev)
		{
			var newBeruf = txt_beruf.Text;
			if (!string.IsNullOrEmpty(newBeruf))
			{
				GetSelectedGast().beruf = newBeruf;
			}
		}

		/// <summary>
		/// EventHandler bei Änderung der Startposition X.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnStartpositionXChanged(object sender, EventArgs ev)
		{
			var selectedGast = GetSelectedGast();
			if (selectedGast != null)
			{
				selectedGast.position = new Point((int)num_startpos_x.Value, selectedGast.position.Y);
			}
			
		}

		/// <summary>
		/// EventHandler bei Änderung der Startposition Y.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnStartpositionYChanged(object sender, EventArgs ev)
		{
			var selectedGast = GetSelectedGast();
			if (selectedGast != null)
			{
				selectedGast.position = new Point(selectedGast.position.X, (int)num_startpos_y.Value);
			}
			
		}

		/// <summary>
		/// EventHandler bei Änderung der Gastselektion für Wunschabstände.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnWunschabstandSelectionChanged(object sender, EventArgs ev)
		{
			var selectedGast = GetSelectedGast();
			var gastForWunschabstand = (Gast)list_gaeste_wunschabstand.SelectedItem;

			num_wunschabstand.Enabled = true;
			int wunschabstand = 0;
			if (!selectedGast.wunschabstaende.TryGetValue(gastForWunschabstand.name, out wunschabstand))
			{
				wunschabstand = 0;
			}
			num_wunschabstand.Value = wunschabstand;
		}

		/// <summary>
		/// EventHandler bei Änderung des Wunschabstandes.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="ev">Eventparameter</param>
		private void OnWunschabstandChanged(object sender, EventArgs ev)
		{
			var selectedGast = GetSelectedGast();
			var gastForWunschabstand = (Gast)list_gaeste_wunschabstand.SelectedItem;

			selectedGast.wunschabstaende[gastForWunschabstand.name] = (int)num_wunschabstand.Value;
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

		/// <summary>
		/// Definiert das Verhalten des OK-Buttons bei Klick.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="e">Eventparameter</param>
		private void Btn_ok_Click(object sender, EventArgs e)
		{
			SaveUiToXml();
			Close();
		}

		/// <summary>
		/// Definiert das Verhalten des Gast-Löschen-Buttins bei Klick.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="e">Eventparameter</param>
		private void Btn_gast_loeschen_Click(object sender, EventArgs e)
		{
			gaesteliste.Remove(GetSelectedGast());
		}

		/// <summary>
		/// Definiert das Verhalten des Gast-Hinzufügen-Buttons bei Klick.
		/// </summary>
		/// <param name="sender">Eventparameter</param>
		/// <param name="e">Eventparameter</param>
		private void Btn_gast_hinzufuegen_Click(object sender, EventArgs e)
		{
			gaesteliste.Add(new Gast("Neuer Gast", "beruf", new Point(0, 0), new Dictionary<string, int>()));
		}
	}
}
