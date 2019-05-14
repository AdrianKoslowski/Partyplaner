using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Partyplaner
{
    public partial class Spielfeld : Form
    {
        Steuerung steuerung;
        public Spielfeld()
        {
            InitializeComponent();
        }

        private void Spielfeld_Load(object sender, EventArgs e)
        {
            steuerung = new Steuerung(this);
            steuerung.Run();
            steuerung.Update();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            steuerung.Update();
        }
    }
}
