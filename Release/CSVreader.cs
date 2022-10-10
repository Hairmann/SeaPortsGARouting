using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Windows;

namespace SeaPortsGenetic_WPF
{
    class CSVreader
    {
        public string path { get; set; }
        public SeaPort[] results { get; private set; }
        private string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string SPGfolder = "\\Sea Ports Genetic";

        public void run()
        {
            path = myDocumentsPath + SPGfolder + "\\bays29.csv";
            using (var reader = new StreamReader(path))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = new List<SeaPort>();
                    csv.Read();
                    csv.ReadHeader();
                    try
                    {
                        while (csv.Read())
                        {
                            var record = new SeaPort();
                            record.setName(csv.GetField("name"));
                            record.setX(csv.GetField<double>("x"));
                            record.setY(csv.GetField<double>("y"));
                            records.Add(record);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка: " + e.Message);
                    }

                    results = records.ToArray();
                }
            }
        }
    }
 }
