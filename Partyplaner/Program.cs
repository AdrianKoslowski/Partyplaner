﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Partyplaner;

namespace Partyplaner
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
           ImportExportHelper ImportHelper = ImportExportHelper.getImportExportHelper();
           
           Console.WriteLine(ImportHelper.GetGast("Marco Polo").beruf);
        }
    }
}
