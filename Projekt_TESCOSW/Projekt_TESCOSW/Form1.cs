using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Projekt_TESCOSW
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            //Vybrání souboru
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"; //Filtr, aby byl soubor .xml
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                //Načtení souboru
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                //Rozdělí data, vypočítá celkovou cenu a celkovou cenu s dph pro každé prodané auto (viz ParseXml)
                List<Sale> sales = ParseXml(xmlDoc);

                //Rozdělení prodaných aut podle názvu (viz AggregateSales)
                var aggregatedSales = AggregateSales(sales);

                //Přidání nových dat do tabulky (viz PopulateGrid)
                PopulateGrid(aggregatedSales);
            }
        }

        private List<Sale> ParseXml(XmlDocument xmlDoc)
        {
            List<Sale> sales = new List<Sale>();

            //Rozdělení a přiřazení dat
            XmlNodeList saleNodes = xmlDoc.SelectNodes("/autosalon/car");
            foreach (XmlNode saleNode in saleNodes)
            {
                string name = saleNode.SelectSingleNode("Name").InnerText;
                DateTime dateofsale = DateTime.Parse(saleNode.SelectSingleNode("Dateofsale").InnerText);
                double price = double.Parse(saleNode.SelectSingleNode("Price").InnerText);
                double dph = double.Parse(saleNode.SelectSingleNode("DPH").InnerText);

                //Přepočítání ceny s dph
                double dph_price = price + (price * (dph / 100));

                //Přidá prodané auto do seznamu
                sales.Add(new Sale { Name = name, DateOfSale = dateofsale, Price = price, DphPrice = dph_price });
            }

            return sales;
        }

        private Dictionary<string, (int Count, double TotalPrice, double TotalPriceDPH)> AggregateSales(List<Sale> sales)
        {
            Dictionary<string, (int Count, double TotalPrice, double TotalPriceDPH)> aggregatedSales = new Dictionary<string, (int, double, double)>();

            //Pro každé prodané auto v seznamu
            foreach (Sale sale in sales)
            {
                if (aggregatedSales.ContainsKey(sale.Name))
                {
                    (int Count, double TotalPrice, double TotalPriceDPH) data = aggregatedSales[sale.Name];
                    aggregatedSales[sale.Name] = (data.Count + 1, data.TotalPrice + sale.Price, data.TotalPriceDPH + sale.DphPrice);
                }
                else
                {
                    aggregatedSales.Add(sale.Name, (1, sale.Price, sale.DphPrice));
                }
            }

            return aggregatedSales;
        }

        private void PopulateGrid(Dictionary<string, (int Count, double TotalPrice, double TotalPriceDPH)> aggregatedSales)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Název", typeof(string));
            dataTable.Columns.Add("Počet prodaných vozů", typeof(int));
            dataTable.Columns.Add("Celková cena", typeof(string));
            dataTable.Columns.Add("Celková cena s DPH", typeof(string));

            foreach (var kvp in aggregatedSales)
            {
                string adjustedPriceFormatted = kvp.Value.TotalPriceDPH.ToString("N2");
                string totalPriceFormatted = kvp.Value.TotalPrice.ToString("N2");


                dataTable.Rows.Add(kvp.Key, kvp.Value.Count, totalPriceFormatted, adjustedPriceFormatted);
            }

            dataGridView.DataSource = dataTable;
        }

        private void button_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }

    class Sale
    {
        public string Name { get; set; }
        public DateTime DateOfSale { get; set; }
        public double Price { get; set; }
        public double DphPrice { get; set; }
    }
}

