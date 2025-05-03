using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AquaPOS
{
    /// <summary>
    /// Interaction logic for Inventory_Management.xaml
    /// </summary>
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
            User_Login loginWindow = new User_Login();
            loginWindow.Show(); // Show the login window
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
            var selectedItem = (sender as Button).DataContext as StockItem;
            if (selectedItem != null)
            {
                txtCategory.Text = selectedItem.Category;
                txtProductName.Text = selectedItem.ProductName;
                txtUnitPrice.Text = selectedItem.UnitPrice.ToString();
                txtQuantity.Text = selectedItem.Quantity.ToString();
                dpDateUpdated.Text = selectedItem.DateUpdated;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as Button).DataContext as StockItem;
            if (selectedItem != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete product '{selectedItem.ProductName}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    StockItems.Remove(selectedItem);
                    RenumberProductIDs();
                    StockDataGrid.Items.Refresh();
                }
            }
        }

        private void txtProductID_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional live lookup or validation
        }

        private void RenumberProductIDs()
        {
            for (int i = 0; i < StockItems.Count; i++)
            {
                StockItems[i].ProductID = i + 1;
            }
        }

        private void txtSearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchProduct.Text == "Enter product name...")
            {
                txtSearchProduct.Text = "";
                txtSearchProduct.Foreground = Brushes.Black;
            }
        }

        private void txtSearchProduct_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchProduct.Text))
            {
                txtSearchProduct.Text = "Enter product name...";
                txtSearchProduct.Foreground = Brushes.Gray;
            }
        }

        private void txtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {

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
            string searchName = txtSearchProduct.Text.Trim();
            if (!string.IsNullOrEmpty(searchName) && searchName != "Enter product name...")
            {
                var product = FindItemByProductName(searchName);

                if (product != null)
                {
                    txtCategory.Text = product.Category;
                    txtProductName.Text = product.ProductName;
                    txtUnitPrice.Text = product.UnitPrice.ToString();
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
                MessageBox.Show("Please enter a product name to search.");
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
                    StockDataGrid.Items.Refresh();
                }
                else
                {
                    // Add new item
                    int nextProductID = StockItems.Count + 1;

                    StockItems.Add(new StockItem
                    {
                        ProductID = nextProductID,
                        Category = txtCategory.Text,
                        ProductName = txtProductName.Text,
                        UnitPrice = double.TryParse(txtUnitPrice.Text, out var price) ? price : 0,
                        Quantity = int.TryParse(txtQuantity.Text, out var qty) ? qty : 0,
                        DateUpdated = DateTime.Now.ToString("yyyy-MM-dd")
                    });

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
