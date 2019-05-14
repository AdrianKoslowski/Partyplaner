﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace Partyplaner
{
    class Darstellung
    {
        Graphics g;
        int gameWidth;
        int gameHeight;
        public const int TILEWIDTH = 25;
        public const int TILEHEIGHT = 25;
        ImportExportHelper DatenManager;
        public const int FORMWIDTH = 500;
        public const int FORMHEIGHT = 500;
        public const int STATISTIKWIDTH = 200;
        public const int STATISTIKHEIGHT = 100;
        Point statistikPosition;
        /// <summary>
        /// Erstellt ei Objekt
        /// </summary>
        /// <param name="g">Von Control.CreateGraphics()</param>
        public Darstellung(Graphics g)
        {
            this.g = g;
            DatenManager = ImportExportHelper.getImportExportHelper();
        }

        public void ZeichneSpielfeld()
        {
            Point tischPos = GetValueFromTupe(DatenManager.GetTischPosition());
            Point tischSize = GetValueFromTupe(DatenManager.GetTischGrosse());
            Point spielFeld = GetValueFromTupe(DatenManager.GetRaum());

            Dictionary<string, Gast> personen = DatenManager.GetGaesteListe();
            gameWidth = spielFeld.X * TILEWIDTH;
            gameHeight = spielFeld.Y * TILEHEIGHT;

            Pen p = new Pen(Color.Black, 1);
            g.Clear(Color.FromKnownColor(KnownColor.Control));
            g.FillRectangle(new Pen(Color.White, 1).Brush, new Rectangle(0, 0, gameWidth, gameHeight));
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);
            for (int x = 0; x <= gameWidth + 1; x += TILEWIDTH)
            {
                startPoint = new Point(x, 0);
                endPoint = new Point(startPoint.X, startPoint.Y + gameHeight);
                g.DrawLine(p, startPoint, endPoint);
            }
            for (int y = 0; y <= gameHeight; y += TILEHEIGHT)
            {
                startPoint = new Point(0, y);
                endPoint = new Point(startPoint.X + gameWidth, startPoint.Y);
                g.DrawLine(p, startPoint, endPoint);
            }

            g.FillRectangle(new Pen(Color.Brown, 1).Brush, new Rectangle(tischPos.X * TILEWIDTH, tischPos.Y * TILEHEIGHT, tischSize.X * TILEWIDTH, tischSize.Y * TILEHEIGHT));


            foreach (var pers in personen)
            {
                Point point = pers.Value.position;
                g.FillRectangle(new Pen(Color.Orange, 1).Brush, new Rectangle(point.X * TILEWIDTH, point.Y * TILEHEIGHT, TILEWIDTH, TILEHEIGHT));
            }
        }

        /// <summary>
        /// Zeichnet die Statistik als Graphen
        /// </summary>
        /// <param name="points">Die Befindlichkeitswerte + Messpunktunkte</param>
        public void DrawPartyIndex(List<Point> points)
        {

            Point grosse = new Point(STATISTIKWIDTH, STATISTIKHEIGHT);
            Pen pGraph = new Pen(Color.Black, 1);

            g.DrawLine(pGraph, new Point(5, 5), new Point(5, 95));
            g.DrawLine(pGraph, new Point(5, 95), new Point(195, 95));

            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                points[i] = new Point((p.X + 1) * 5, p.Y);
            }
            if (points.Count > 0)
                g.DrawLines(pGraph, points.ToArray());
        }

        private Point GetValueFromTupe(Tuple<int, int> tuple)
        {
            return new Point(tuple.Item1, tuple.Item2);
        }
        public void ResizeGameField(Spielfeld spielfeld)
        {
            //Passe Form auf Spielfeldgröße an
            if (CalculateWidth() > FORMWIDTH)
            {
                spielfeld.Size = new Size(CalculateWidth(), FORMHEIGHT);
                if (CalculateHeight() > FORMHEIGHT)
                {
                    spielfeld.Size = new Size(CalculateWidth(), CalculateHeight());
                }
            }
            else
            {
                spielfeld.Size = new Size(FORMWIDTH, FORMHEIGHT);
                if (CalculateHeight() > FORMHEIGHT)
                {
                    spielfeld.Size = new Size(FORMWIDTH, CalculateHeight());
                }
            }
            statistikPosition = new Point(spielfeld.Size.Width - STATISTIKWIDTH,spielfeld.Size.Height - STATISTIKWIDTH);
            g = spielfeld.CreateGraphics();
        }

        private int CalculateWidth()
        {
            return (gameWidth * TILEWIDTH) + STATISTIKWIDTH;
        }
        private int CalculateHeight()
        {
            return (gameHeight * TILEHEIGHT) + STATISTIKHEIGHT;
        }
    }
}
