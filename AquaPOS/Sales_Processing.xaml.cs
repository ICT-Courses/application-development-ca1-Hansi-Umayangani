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
        private readonly string connectionString = "Data Source=C:\\Projects\\Appication Development\\application-development-ca1-Hansi-Umayangani\\AquaPOS\\Databases\\AquaPOS-Database.db";
        private readonly List<Product> productList = new List<Product>();
        private readonly ObservableCollection<BillingItem> billItems = new ObservableCollection<BillingItem>();

        public Sales_Processing()
        {
            InitializeComponent();
            LoadProductsFromDatabase();
            LoadProductNamesFromDatabase();

            BillDataGrid.ItemsSource = billItems;

        }

        private void CmbSearchProduct_SelectionChanged(object sender, RoutedEventArgs e)
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

        private void CmbSearchProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void CmbSearchProduct_Loaded(object sender, RoutedEventArgs e)
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


        private void CmbSearchProductDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void CmbSearchProductDetails_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void CmbSearchProductDetails_Loaded(object sender, RoutedEventArgs e)
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

        private void TxtProductNameResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in ProductName textbox
        }

        private void TxtCategoryResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in Category textbox
        }

        private void TxtUnitPriceResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: React to changes in UnitPrice textbox
        }

        private void TxtAvailableQtyResult_TextChanged(object sender, TextChangedEventArgs e)
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

                if (billItems.Count == 0)
                {
                    MessageBox.Show("No items to print in the bill.");
                    return;
                }

                try
                {
                    List<Sale> cartItems = new List<Sale>();
                    var saleDateTime = DateTime.Now.ToString("yyyy-MM-dd  HH:mm");

                    foreach (var item in billItems)
                    {

                        var sale = new Sale
                        {
                            ProductID = item.ProductID,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalPrice,
                            SaleDate = saleDateTime
                        };

                        cartItems.Add(sale);
                        LoadProductsFromDatabase();
                        LoadProductNamesFromDatabase();
                    }

                    DatabaseInitializer.RecordSale(cartItems, Convert.ToDouble(TotalAmountTextBlock.Text), saleDateTime);


                    // Create new PDF document
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Sales Bill";

                    // Add a page
                    PdfPage page = document.AddPage();

                    // Create graphics object for drawing
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont headerFont = new XFont("Times New Roman", 16, XFontStyleEx.Bold);
                    XFont subHeaderFont = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
                    XFont bodyFont = new XFont("Times New Roman", 10, XFontStyleEx.Regular);

                    double yPoint = 40;

                    // Add Bill Header
                    gfx.DrawString("Sales Bill", headerFont, XBrushes.Black, new XRect(0, yPoint, page.Width.Point, page.Height.Point), XStringFormats.TopCenter);
                    yPoint += 40;

                    // Add Bill Date
                    XFont boldFont = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
                    XFont regularFont = new XFont("Times New Roman", 12, XFontStyleEx.Regular);

                    string labelText = "Bill Date:";
                    string dateText = DateTime.Now.ToString("yyyy-MM-dd   HH:mm");

                    var labelSize = gfx.MeasureString(labelText, boldFont);
                    var dateSize = gfx.MeasureString(dateText, regularFont);

                    double dateX = page.Width.Point - 40 - dateSize.Width;
                    double labelX = dateX - 5 - labelSize.Width;  // 5 px gap between label and date

                    gfx.DrawString(labelText, boldFont, XBrushes.Black, new XPoint(labelX, yPoint), XStringFormats.TopLeft);
                    gfx.DrawString(dateText, regularFont, XBrushes.Black, new XPoint(dateX, yPoint), XStringFormats.TopLeft);

                    yPoint += 40;

                    // Add table headers
                    gfx.DrawString("Product Name", subHeaderFont, XBrushes.Black, new XRect(40, yPoint, 160, page.Height.Point), XStringFormats.TopCenter);
                    gfx.DrawString("Unit Price (Rs.)", subHeaderFont, XBrushes.Black, new XRect(200, yPoint, 140, page.Height.Point), XStringFormats.TopCenter);
                    gfx.DrawString("Quantity", subHeaderFont, XBrushes.Black, new XRect(340, yPoint, 90, page.Height.Point), XStringFormats.TopCenter);
                    gfx.DrawString("Total Price (Rs.)", subHeaderFont, XBrushes.Black, new XRect(430, yPoint, 120, page.Height.Point), XStringFormats.TopCenter);

                    yPoint += 25;

                    // Draw a line under the header
                    gfx.DrawLine(XPens.Black, 40, yPoint, page.Width.Point - 40, yPoint);
                    yPoint += 10;

                    // Add bill items
                    foreach (var item in billItems)
                    {
                        gfx.DrawString(item.ProductName, bodyFont, XBrushes.Black, new XRect(40, yPoint, 160, page.Height.Point), XStringFormats.TopCenter);
                        gfx.DrawString(item.UnitPrice.ToString("F2"), bodyFont, XBrushes.Black, new XRect(200, yPoint, 140, page.Height.Point), XStringFormats.TopCenter);
                        gfx.DrawString(item.Quantity.ToString(), bodyFont, XBrushes.Black, new XRect(340, yPoint, 90, page.Height.Point), XStringFormats.TopCenter);
                        gfx.DrawString(item.TotalPrice.ToString("F2"), bodyFont, XBrushes.Black, new XRect(430, yPoint, 120, page.Height.Point), XStringFormats.TopCenter);

                        yPoint += 20;

                        // Check if we need to add a new page if space runs out
                        if (yPoint > page.Height.Point - 50)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPoint = 40;
                        }
                    }

                    // Draw a line above the total
                    yPoint += 20;
                    gfx.DrawLine(XPens.Black, 40, yPoint, page.Width.Point - 40, yPoint);
                    yPoint += 10;

                    //Calculate Total Amount
                    double totalAmount = 0;
                    foreach (var item in billItems)
                    {
                        totalAmount += item.TotalPrice;
                    }

                    // Prepare total amount text
                    string totalAmountText = "Total Amount (Rs.):";
                    string amountText = $"{totalAmount:F2}";
                    var textSize = gfx.MeasureString(totalAmountText, subHeaderFont);
                    var amountSize = gfx.MeasureString(amountText, subHeaderFont);

                    double amountX = page.Width.Point - 40 - amountSize.Width;
                    double textX = amountX - 10 - labelSize.Width; // 10 px gap between label and amount

                    gfx.DrawString(totalAmountText, subHeaderFont, XBrushes.Black, new XPoint(labelX, yPoint), XStringFormats.TopLeft);
                    gfx.DrawString(amountText, subHeaderFont, XBrushes.Black, new XPoint(amountX, yPoint), XStringFormats.TopLeft);

                    // Draw double underline under the amount
                    double underlineY1 = yPoint + amountSize.Height + 2; // first line slightly below text
                    double underlineY2 = underlineY1 + 2;               // second line slightly below the first
                    gfx.DrawLine(XPens.Black, amountX, underlineY1, amountX + amountSize.Width, underlineY1);
                    gfx.DrawLine(XPens.Black, amountX, underlineY2, amountX + amountSize.Width, underlineY2);

                    // Save PDF
                    string filename = $"SalesBill_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
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

    public class Sale
    {
        public int SaleID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public string SaleDate { get; set; }
    }
}
