using System;
using System.Collections.Generic;
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
    /// Interaction logic for LowStockWindow.xaml
    /// </summary>
    public partial class LowStockWindow : Window
    {
        public LowStockWindow()
        {
            InitializeComponent();
            LoadLowStockItems();
        }

        private void LoadLowStockItems()
        {
            List<StockItem> lowStockItems = DatabaseInitializer.GetLowStockItems();

            // Debug output
            foreach (var item in lowStockItems)
            {
                Console.WriteLine($"{item.ProductName} - Quantity: {item.Quantity}");
            }

            LowStockDataGrid.ItemsSource = lowStockItems;
        }

        private void LowStockDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
