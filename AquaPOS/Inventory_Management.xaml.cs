using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.Data.SQLite;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace AquaPOS
{
    public partial class Inventory_Management : Window
    {
        public ObservableCollection<StockItem> StockItems { get; set; }

        public Inventory_Management()
        {
            InitializeComponent();

            StockItems = new ObservableCollection<StockItem>
            {
                new StockItem { Category = "Fish", ProductID = 0001, ProductName = "Goldfish", UnitPrice = 200.00, Quantity = 10, DateUpdated = DateTime.Now.ToString("yyyy-MM-dd") }
            };

            StockDataGrid.ItemsSource = StockItems;
            LoadStockItemsFromDatabase();
            LoadStockItems();
            PopulateProductComboBox();
        }

        private void LoadStockItemsFromDatabase()
        {
            StockItems.Clear();
            using (var conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM StockItems;";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StockItems.Add(new StockItem
                            {
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                Category = reader["Category"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                UnitPrice = Convert.ToDouble(reader["UnitPrice"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                DateUpdated = reader["DateUpdated"].ToString()
                            });
                        }
                    }
                }
            }
        }

        private void PopulateProductComboBox()
        {
            var productNames = StockItems.Select(item => item.ProductName).ToList();
            cmbSearchProduct.ItemsSource = productNames;
        }


        private void LoadStockItems()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                conn.Open();
                string query = "SELECT ProductName FROM Products";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                List<string> products = new List<string>();
                while (reader.Read())
                {
                    products.Add(reader["ProductName"].ToString());
                }

                cmbSearchProduct.ItemsSource = products;
            }
        }


        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        private void InventoryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            Inventory_Management inventory_Management = new Inventory_Management();
            inventory_Management.Show();
            this.Close();
        }

        private void SalesReportsButton_Click(object sender, RoutedEventArgs e)
        {
            Sales_Reports sales_Reports = new Sales_Reports();
            sales_Reports.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            User_Login user_Login = new User_Login();
            user_Login.Show(); // Show the login window
            this.Hide(); // Close the current window
        }

        private void StockDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (StockDataGrid.SelectedItem is StockItem selectedItem)
            {
                txtCategory.Text = selectedItem.Category;
                txtProductName.Text = selectedItem.ProductName;
                txtUnitPrice.Text = selectedItem.UnitPrice.ToString();
                txtQuantity.Text = selectedItem.Quantity.ToString();
                dpDateUpdated.Text = selectedItem.DateUpdated;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StockItem itemToEdit)
            {
                txtCategory.Text = itemToEdit.Category;
                txtProductName.Text = itemToEdit.ProductName;
                txtUnitPrice.Text = itemToEdit.UnitPrice.ToString();
                txtQuantity.Text = itemToEdit.Quantity.ToString();
                dpDateUpdated.Text = itemToEdit.DateUpdated;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as Button).DataContext as StockItem;
            if (sender is Button button && button.DataContext is StockItem itemToDelete)
            {
                var result = MessageBox.Show($"Are you sure you want to delete product '{itemToDelete.ProductName}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    StockItems.Remove(selectedItem);
                    RenumberProductIDs();
                    StockDataGrid.Items.Refresh();
                }
            }
        }

        private void RenumberProductIDs()
        {
            for (int i = 0; i < StockItems.Count; i++)
            {
                StockItems[i].ProductID = i + 1;
            }
        }

        private StockItem FindItemByProductName(string productName)
        {
            foreach (var item in StockItems)
            {
                if (item.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase))
                    return item;
            }
            return null;
        }

        private void SearchProduct_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSearchProduct.SelectedItem != null)
            {
                string selectedProductName = cmbSearchProduct.Text;
                var product = FindItemByProductName(selectedProductName);

                if (product != null)
                {
                    txtCategory.Text = product.Category;
                    txtProductName.Text = product.ProductName;
                    txtUnitPrice.Text = product.UnitPrice.ToString("F2");
                    txtQuantity.Text = product.Quantity.ToString();
                    dpDateUpdated.Text = product.DateUpdated;
                }
                else
                {
                    MessageBox.Show("Product not found.");
                }
            }
            else
            {
                MessageBox.Show("Please select or enter a product name to search.");
            }
        }

        private void CmbSearchProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void CmbSearchProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Optional: Handle preview text input in combo box
        }

        private void CmbSearchProduct_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: Handle when the combo box is loaded
        }


        private void ClearFields()
        {
            txtCategory.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            dpDateUpdated.SelectedDate = null;
        }

        private void UpdateStock_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                var existingItem = FindItemByProductName(txtProductName.Text);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Category = txtCategory.Text;
                    existingItem.UnitPrice = double.TryParse(txtUnitPrice.Text, out var price) ? price : 0;
                    existingItem.Quantity = int.TryParse(txtQuantity.Text, out var qty) ? qty : 0;
                    existingItem.DateUpdated = dpDateUpdated.Text;

                    DatabaseInitializer.UpdateStockItem(existingItem);
                    StockDataGrid.Items.Refresh();
                }
                else
                {
                    // Add new item
                    int nextProductID = StockItems.Count + 1;

                    var newItem = new StockItem
                    {
                        ProductID = nextProductID,
                        Category = txtCategory.Text,
                        ProductName = txtProductName.Text,
                        UnitPrice = double.TryParse(txtUnitPrice.Text, out var price) ? price : 0,
                        Quantity = int.TryParse(txtQuantity.Text, out var qty) ? qty : 0,
                        DateUpdated = DateTime.Now.ToString("yyyy-MM-dd")
                    };

                    DatabaseInitializer.AddStockItem(newItem);
                    StockItems.Add(newItem);
                    StockDataGrid.Items.Refresh();
                }

                ClearFields();
            }
            else
            {
                MessageBox.Show("Product Name is required.");
            }
        }

        private void PrintStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Stock Report";
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Fonts and styles
                XFont titleFont = new XFont("Times New Roman", 16, XFontStyleEx.Bold);
                XFont headerFont = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
                XFont regularFont = new XFont("Times New Roman", 10, XFontStyleEx.Regular);

                // Page dimensions
                XUnit pageWidth = page.Width;
                XUnit pageHeight = page.Height;

                // Margins and table settings
                double leftMargin = 40;
                int numColumns = 6;
                double tableWidth = pageWidth.Point - 2 * leftMargin;
                double columnWidth = tableWidth / numColumns;

                // Starting Y position
                double yPoint = 40;

                // Draw title (centered)
                gfx.DrawString(
                    "Stock Report",
                    titleFont,
                    XBrushes.Black,
                    new XRect(0, yPoint, pageWidth.Point, 30), // XRect width should use the actual point value
                    XStringFormats.TopCenter
                );

                yPoint += 40; // Space below title

                // Define headers
                string[] headers = { "ID", "Category", "Product Name", "Price (Rs.)", "Qantity", "Date Updated" };

                // Draw table header
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(
                        headers[i],
                        headerFont,
                        XBrushes.Black,
                        new XRect(leftMargin + i * columnWidth, yPoint, columnWidth, 20),
                        XStringFormats.TopCenter
                    );
                }

                yPoint += 20;

                // Draw underline below header
                gfx.DrawLine(
                    XPens.Black,
                    new XUnitPt(leftMargin),
                    new XUnitPt(yPoint),
                    new XUnitPt(pageWidth.Point - leftMargin),
                    new XUnitPt(yPoint)
                );

                yPoint += 20; // Gap before data starts

                int pageNumber = 1;

                foreach (var item in StockItems)
                {
                    bool isQtyNumber = int.TryParse(item.Quantity.ToString(), out int qtyVal);
                    XBrush brush = (isQtyNumber && qtyVal < 5) ? XBrushes.Red : XBrushes.Black;

                    string priceText = double.TryParse(item.UnitPrice.ToString(), out double priceVal)
                        ? priceVal.ToString("F2")
                        : item.UnitPrice.ToString();

                    string quantityText = item.Quantity.ToString();

                    string dateText;
                    if (DateTime.TryParse(item.DateUpdated?.ToString(), out DateTime dateVal))
                    {
                        dateText = dateVal.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        dateText = item.DateUpdated?.ToString();
                    }

                    // Prepare row data
                    string[] dataItems = {
                        item.ProductID.ToString(),
                        item.Category?.ToString(),
                        item.ProductName?.ToString(),
                        priceText,
                        quantityText,
                        dateText
                    };

                    // Draw each cell
                    for (int i = 0; i < dataItems.Length; i++)
                    {
                    gfx.DrawString(
                        dataItems[i],
                        regularFont,
                        (i == 4) ? brush : XBrushes.Black, // Qty column in red if low stock
                        new XRect(leftMargin + i * columnWidth, yPoint, columnWidth, 15),
                        XStringFormats.TopCenter
                        );
                    }

                    yPoint += 15;

                    // Line between rows
                    gfx.DrawLine(
                        XPens.LightGray,
                        new XUnitPt(leftMargin),
                        new XUnitPt(yPoint),
                        new XUnitPt(pageWidth.Point - leftMargin),
                        new XUnitPt(yPoint)
                        );
                }

                    // Footer: page number
                    gfx.DrawString(
                        $"Page {pageNumber}",
                        regularFont,
                        XBrushes.Gray,
                        new XRect(0, pageHeight.Point - 40, pageWidth.Point, 20),
                        XStringFormats.TopCenter
                        );

                    // Save and open the document
                    string filename = "StockReport.pdf";
                    document.Save(filename);
                    Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
                    }

            catch (Exception ex)
            {
                MessageBox.Show("Error generating PDF: " + ex.Message);
            }
        }
    }


        public class StockItem
    {
        public int ProductID { get; set; } // Auto-generated index
        public string Category { get; set; }
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string DateUpdated { get; set; }
    }
}
