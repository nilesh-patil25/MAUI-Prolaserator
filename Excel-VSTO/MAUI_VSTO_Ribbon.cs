using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Excel_VSTO
{
    public partial class MAUI_VSTO_Ribbon
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            this.saveCurrentCSV();

            string command = ConfigurationManager.AppSettings["ApplicationMSIX"];
            Process.Start(command);

        }
        private void saveCurrentCSV()
        {
            try
            {
                // Get a reference to the active Excel application
                Microsoft.Office.Interop.Excel.Application excelApp = Globals.ThisAddIn.Application;

                // Get the active workbook
                Microsoft.Office.Interop.Excel.Workbook activeWorkbook = excelApp.ActiveWorkbook;

                if (activeWorkbook != null)
                {
                    string folderPath = ConfigurationManager.AppSettings["productRoot"]; // Change this to your desired folder path
                    string fileName = $"ExportedCSV_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"; // Change this to your desired file name

                    string filePath = System.IO.Path.Combine(folderPath, fileName);

                    // Save the active workbook as CSV format
                    activeWorkbook.SaveAs(filePath, Microsoft.Office.Interop.Excel.XlFileFormat.xlCSV);
                    activeWorkbook.Close(SaveChanges: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

        }
    }
}
