using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Management;

namespace printerAPI
{
    class PrintManager
    {

        protected string defaultPrinter;

        public PrinterSettings.StringCollection installed;

        public PrintManager()
        {
            this.installed = PrinterSettings.InstalledPrinters;
            this.getDefaultPrinter();
        }

        public void setDefaultPrinter(string printerName)
        {
            SetDefaultPrinter(printerName);
        }

        public string getDefaultPrinter()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer");
            var searcher = new ManagementObjectSearcher(query);


            foreach (ManagementObject mo in searcher.Get())
            {
                if (((bool?)mo["Default"]) ?? false)
                {
                   this.defaultPrinter =  mo["Name"] as string;
                   return this.defaultPrinter;
                }
            }

            return null;
        }

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern bool SetDefaultPrinter(string Name);

    }
}
