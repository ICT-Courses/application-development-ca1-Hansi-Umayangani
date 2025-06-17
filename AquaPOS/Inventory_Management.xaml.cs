using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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

        private void LoadStockItems()
        {
            using (SQLiteConnection conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                conn.Open();
                string query = "SELECT ProductName FROM StockItems";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                List<string> products = new List<string>();
                while (reader.Read())
                {
                    products.Add(reader["ProductName"].ToString());
                }

                cmbSearchProduct.ItemsSource = StockItems;
            }
        }

        private void PopulateProductComboBox()
        {
            var productNames = StockItems.Select(item => item.ProductName).ToList();
            cmbSearchProduct.ItemsSource = productNames;
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
            string enteredProductName = cmbSearchProduct.Text.Trim();

            if (string.IsNullOrWhiteSpace(enteredProductName))
            {
                MessageBox.Show("Please enter a product name to search.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
                return;
            }

            var product = FindItemByProductName(enteredProductName);

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
                MessageBox.Show("Product not found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearFields();
            }
        }

        private void ClearFields()
        {
            txtCategory.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            dpDateUpdated.SelectedDate = null;
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

                    if (int.TryParse(txtQuantity.Text, out var qty))
                    {
                        existingItem.Quantity += qty;
                    }

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

                // Margins and metrics
                XUnit leftMargin = XUnit.FromPoint(40);
                XUnit y = XUnit.FromPoint(40);
                double pageWidthPt = page.Width.Point;
                double usableWidth = pageWidthPt - 2 * leftMargin.Point;
                int columns = 7;
                double colWidthPt = usableWidth / columns;

                // Draw report title
                gfx.DrawString("Stock Report", titleFont, XBrushes.Black,
                    new XRect(0, y.Point, page.Width.Point, XUnit.FromPoint(20).Point), XStringFormats.TopCenter);

                y += XUnit.FromPoint(40);

                // Table headers
                string[] headers = { "ID", "Category", "Product Name", "Quantity", "Unit Price (Rs.)", "Last Update On" };
                double[] columnWidths = {
                    40,  // ID
                    80,  // Category
                    120, // Product Name
                    100,  // Quantity
                    100,  // Unit Price
                    100   // Last Update On
                };
                columns = headers.Length;

                double xPos = leftMargin.Point;

                for (int i = 0; i < columns; i++)
                {
                    gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                        new XRect(xPos, y.Point, columnWidths[i], 20),
                        XStringFormats.TopCenter);

                    xPos += columnWidths[i];
                }
                y += XUnit.FromPoint(20);

                double tableStartX = leftMargin.Point;
                double tableWidth = columnWidths.Sum();

                // Underline
                gfx.DrawLine(XPens.Black, tableStartX, y.Point, tableStartX + tableWidth, y.Point);
                y += XUnit.FromPoint(10);

                int rowCount = StockItems.Count;
                int currentRowIndex = 0;

                // Table rows
                foreach (var item in StockItems)
                {
                    string[] row = {
                    item.ProductID.ToString(),
                    item.Category ?? "",
                    item.ProductName ?? "",
                    item.Quantity.ToString(),
                    $"Rs. {item.UnitPrice:F2}",
                    item.DateUpdated
                    };

                    double rowX = leftMargin.Point;

                    for (int i = 0; i < columns; i++)
                    {
                        gfx.DrawString(
                            row[i],
                            regularFont,
                            XBrushes.Black,
                            new XRect(rowX, y.Point, columnWidths[i], 20),
                            XStringFormats.TopCenter);

                        rowX += columnWidths[i];
                    }

                    y += XUnit.FromPoint(15); 

                    // Draw horizontal line UNDER the row — only if NOT the last row
                    if (currentRowIndex < rowCount - 1)
                    {
                        gfx.DrawLine(
                            XPens.LightGray,
                            new XPoint(tableStartX, y.Point),
                            new XPoint(tableStartX + tableWidth, y.Point)
                        );
                    }

                    currentRowIndex++;

                    // Page break check
                    if (y.Point > page.Height.Point - 60)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = XUnit.FromPoint(40);

                        // Re-draw table header on new page
                        double headerX = leftMargin.Point;
                        for (int i = 0; i < columns; i++)
                        {
                            gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                new XRect(headerX, y.Point, columnWidths[i], 20),
                                XStringFormats.TopCenter);
                            headerX += columnWidths[i];
                        }
                        y += XUnit.FromPoint(30);
                    }
                }

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
    }


    public class StockItem
    {
        public int ProductID { get; set; }
        public string Category { get; set; }
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string DateUpdated { get; set; }
    }
}
