using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;

namespace AquaPOS
{
    public partial class Sales_Processing : Window
    {
        private string connectionString = "Data Source=C:\\Projects\\Appication Development\\application-development-ca1-Hansi-Umayangani\\AquaPOS\\Databases\\AquaPOS-Database.db";
        private List<Product> productList = new List<Product>();
        private List<BillingItem> billingItems = new List<BillingItem>();

        private ObservableCollection<BillingItem> billItems = new ObservableCollection<BillingItem>();

        public Sales_Processing()
        {
            InitializeComponent();
            LoadProductsFromDatabase();
            LoadProductNamesFromDatabase();

            BillDataGrid.ItemsSource = billItems;

        }

        private void cmbSearchProduct_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Handle the search functionality when the search button is clicked
            if (cmbSearchProduct.SelectedItem != null)
            {
                string selectedProductName = cmbSearchProduct.SelectedItem.ToString();
                var selectedProduct = productList.Find(p => p.ProductName == selectedProductName);

                if (selectedProduct != null)
                {
                    // Update text boxes with selected product details
                    txtProductNameResult.Text = selectedProduct.ProductName;
                    txtCategoryResult.Text = selectedProduct.Category;
                    txtUnitPriceResult.Text = selectedProduct.UnitPrice.ToString("F2");
                    txtAvailableQtyResult.Text = selectedProduct.AvailableQty.ToString();
                }
            }
            else
            {
                txtProductNameResult.Clear();
                txtCategoryResult.Clear();
                txtUnitPriceResult.Clear();
                txtAvailableQtyResult.Clear();
            }
        }

        private void cmbSearchProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void cmbSearchProduct_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: Handle when the combo box is loaded
        }

        private void LoadProductNamesFromDatabase()
        {
            productList.Clear();
            cmbSearchProduct.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ProductID, ProductName, Category, UnitPrice, Quantity FROM StockItems";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    ProductID = int.Parse(reader["ProductID"].ToString()),
                                    ProductName = reader["ProductName"].ToString(),
                                    Category = reader["Category"].ToString(),
                                    UnitPrice = double.Parse(reader["UnitPrice"].ToString()),
                                    AvailableQty = int.Parse(reader["Quantity"].ToString())
                                };
                                productList.Add(product);
                                cmbSearchProduct.Items.Add(product.ProductName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading products: " + ex.Message);
                }
            }
        }


        private void cmbSearchProductDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSearchProductDetails.SelectedItem != null)
            {
                string selectedProductName = cmbSearchProductDetails.SelectedItem.ToString();
                var selectedProduct = productList.Find(p => p.ProductName == selectedProductName);

                if (selectedProduct != null)
                {
                    txtProductNameResult.Text = selectedProduct.ProductName;
                    txtCategoryResult.Text = selectedProduct.Category;
                    txtUnitPriceResult.Text = selectedProduct.UnitPrice.ToString("F2");
                    txtAvailableQtyResult.Text = selectedProduct.AvailableQty.ToString();
                }
            }
            else
            {
                txtProductNameResult.Clear();
                txtCategoryResult.Clear();
                txtUnitPriceResult.Clear();
                txtAvailableQtyResult.Clear();
            }
        }

        private void cmbSearchProductDetails_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void cmbSearchProductDetails_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: Handle when the combo box is loaded
        }

        private void LoadProductsFromDatabase()
        {
            productList.Clear();
            cmbSearchProductDetails.Items.Clear();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ProductID, ProductName, Category, UnitPrice, Quantity FROM StockItems";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    ProductID = int.Parse(reader["ProductID"].ToString()),
                                    ProductName = reader["ProductName"].ToString(),
                                    Category = reader["Category"].ToString(),
                                    UnitPrice = double.Parse(reader["UnitPrice"].ToString()),
                                    AvailableQty = int.Parse(reader["Quantity"].ToString())
                                };
                                productList.Add(product);
                                cmbSearchProductDetails.Items.Add(product.ProductName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading products: " + ex.Message);
                }
            }
        }

        private void txtProductNameResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in ProductName textbox
        }

        private void txtCategoryResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in Category textbox
        }

        private void txtUnitPriceResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in UnitPrice textbox
        }

        private void txtAvailableQtyResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in AvailableQty textbox
        }

        private void AddToBillButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSearchProduct.SelectedItem != null && int.TryParse(QuantityTextBox.Text, out int qty) && qty > 0)
            {
                string selectedProductName = cmbSearchProduct.SelectedItem.ToString();
                var selectedProduct = productList.Find(p => p.ProductName == selectedProductName);

                if (selectedProduct != null)
                {
                    var billItem = new BillingItem
                    {
                        ProductID = selectedProduct.ProductID, // You will need to load ProductID from DB too
                        ProductName = selectedProduct.ProductName,
                        UnitPrice = selectedProduct.UnitPrice,
                        Quantity = qty
                    };

                    billItems.Add(billItem);
                    UpdateTotalAmount();
                }
            }
            else
            {
                MessageBox.Show("Please select a product and enter a valid quantity.");
            }
        }

        private void UpdateTotalAmount()
        {
            double totalAmount = 0;
            foreach (var item in billItems)
            {
                totalAmount += item.TotalPrice;
            }
            TotalAmountTextBlock.Text = totalAmount.ToString("F2");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is BillingItem item)
            {
                billItems.Remove(item);
                UpdateTotalAmount();
            }
        }

        private void StockDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Handle selection change in the stock data grid
        }

        private void PrintBillButton_Click(object sender, RoutedEventArgs e)
        {
            {
                if (billItems.Count == 0)
                {
                    MessageBox.Show("No items to print in the bill.");
                    return;
                }

                try
                {
                    foreach (var item in billItems)
                    {
                        var sale = new Sale
                        {
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalPrice,
                            SaleDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        DatabaseInitializer.RecordSale(sale);
                        LoadProductsFromDatabase();
                        LoadProductNamesFromDatabase();
                    }

                    // Create new PDF document
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Sales Bill";

                    // Add a page
                    PdfPage page = document.AddPage();

                    // Create graphics object for drawing
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont headerFont = new XFont("Verdana", 16, XFontStyleEx.Bold);
                    XFont subHeaderFont = new XFont("Verdana", 12, XFontStyleEx.Bold);
                    XFont bodyFont = new XFont("Verdana", 10, XFontStyleEx.Regular);

                    double yPoint = 40;

                    // Add Bill Header
                    gfx.DrawString("Sales Bill", headerFont, XBrushes.Black, new XRect(0, yPoint, page.Width, page.Height), XStringFormats.TopCenter);
                    yPoint += 40;

                    // Add table headers
                    gfx.DrawString("Product Name", subHeaderFont, XBrushes.Black, new XRect(40, yPoint, 150, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString("Unit Price (Rs.)", subHeaderFont, XBrushes.Black, new XRect(200, yPoint, 80, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString("Quantity", subHeaderFont, XBrushes.Black, new XRect(290, yPoint, 80, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString("Total Price (Rs.)", subHeaderFont, XBrushes.Black, new XRect(380, yPoint, 100, page.Height), XStringFormats.TopLeft);

                    yPoint += 25;

                    // Draw a line under the header
                    gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                    yPoint += 10;

                    // Add bill items
                    foreach (var item in billItems)
                    {
                        gfx.DrawString(item.ProductName, bodyFont, XBrushes.Black, new XRect(40, yPoint, 150, page.Height), XStringFormats.TopLeft);
                        gfx.DrawString(item.UnitPrice.ToString("F2"), bodyFont, XBrushes.Black, new XRect(200, yPoint, 80, page.Height), XStringFormats.TopLeft);
                        gfx.DrawString(item.Quantity.ToString(), bodyFont, XBrushes.Black, new XRect(290, yPoint, 80, page.Height), XStringFormats.TopLeft);
                        gfx.DrawString(item.TotalPrice.ToString("F2"), bodyFont, XBrushes.Black, new XRect(380, yPoint, 100, page.Height), XStringFormats.TopLeft);

                        yPoint += 20;

                        // Check if the page is full
                        if (yPoint > page.Height - 50)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPoint = 40;
                        }
                    }

                    // Draw total amount
                    yPoint += 20;
                    gfx.DrawLine(XPens.Black, 40, yPoint, page.Width - 40, yPoint);
                    yPoint += 10;
                    double totalAmount = 0;
                    foreach (var item in billItems)
                    {
                        totalAmount += item.TotalPrice;
                    }
                    gfx.DrawString($"Total Amount: {totalAmount.ToString("F2")}", subHeaderFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);

                    // Save PDF
                    string filename = $"Bill_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf";
                    document.Save(filename);
                    billItems.Clear();
                    QuantityTextBox.Clear();
                    totalAmount = 0;
                    TotalAmountTextBlock.Text = totalAmount.ToString("F2");


                    // Optionally open the PDF
                    Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });

                    MessageBox.Show("Bill has been printed successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while printing the bill: " + ex.Message);
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            User_Login user_Login = new User_Login();
            user_Login.Show();
            this.Hide();
        }

        private void SearchProduct_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchProductDetails_Click(object sender, RoutedEventArgs e)
        {

        }

       
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public double UnitPrice { get; set; }
        public int AvailableQty { get; set; }
    }

    public class BillingItem : INotifyPropertyChanged
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => UnitPrice * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Sale class to represent a sale record
    public class Sale
    {
        public int SaleID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public string SaleDate { get; set; }
    }
}
