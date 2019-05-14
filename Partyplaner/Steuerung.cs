﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partyplaner
{
    class Steuerung
    {

        ImportExportHelper importExportHelper;
        Spielfeld spielfeld;
        Darstellung dar;
        Dictionary<string, Gast> liste;
        List<Point> tisch = new List<Point>();
        Tuple<int, int> raum;
        Statistik statistik;
        public Steuerung(Spielfeld spielfeld)
        {
            importExportHelper = ImportExportHelper.getImportExportHelper();
            dar = new Darstellung(spielfeld.CreateGraphics());
            this.spielfeld = spielfeld;
            dar.ResizeGameField(spielfeld);
            statistik = new Statistik();
        }

        public void Run()
        {

            liste = importExportHelper.GetGaesteListe();
            raum = importExportHelper.GetRaum();
            var tStart = importExportHelper.GetTischPosition();
            var tGroe = importExportHelper.GetTischGrosse();
            Tuple<int, int> tEnde = new Tuple<int, int>(tStart.Item1 + tGroe.Item1, tStart.Item2 + tGroe.Item2);
            //List<Point> tisch = new List<Point>();
            for (int a = tStart.Item1; a <= tEnde.Item1; a++)
            {
                for (int b = tStart.Item2; b <= tEnde.Item2; b++)
                {
                    Point pkt = new Point(a, b);
                    tisch.Add(pkt);
                }
            }


        }

        public void Update()
        {
            foreach (Gast person in liste.Values)
            {
                BewegePerson(person, liste, tisch, raum);
            }
            
            float partyIndex = statistik.GetPartyindex();
            dar.ZeichneSpielfeld();
        }

        public void BewegePerson(Gast person, Dictionary<string, Gast> liste, List<Point> tisch, Tuple<int, int> raum)
        {
            //Die Befindlichkeiten der umliegenden Punkte werden abgerufen
            
            Dictionary<Point, double> befi = Befindlichkeit.GetBefindlichkeit(person, liste.Values.ToList());

            //Nicht zugängliche Punkte werden ausgeschlossen
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int xakt = person.position.X + x;
                    int yakt = person.position.Y + y;
                    Point pakt = new Point(xakt, yakt);

                    //Prüfung auf Wand
                    if(xakt <0 || yakt < 0 || xakt> raum.Item1 || yakt>raum.Item2)
                    {
                        befi[pakt] = 999;
                        continue;
                    }

                    //Prüfung auf Tisch
                    foreach (Point pkt in tisch)
                    {
                        if (pakt == pkt)
                        {
                            befi[pakt] = 999;
                            break;
                        }
                    }

                    if(befi[pakt] == 999) { continue; }

                    //Prüfung, ob Punkt bereits besetzt ist
                    foreach (Gast gast in liste.Values)
                    {
                        if (xakt == gast.position.X && yakt == gast.position.Y && gast != person)
                        {
                            befi[pakt] = 999;
                            break;
                        }
                    }
                }
            }

            //der kleinste Wert der Befindlichkeiten ist gesucht
            var keyAndValue = befi.OrderBy(kvp => kvp.Value).First();
            
            //Schließlich wird die Person auf die neue Position gesetzt
            person.position = keyAndValue.Key;

        }
    }
}
