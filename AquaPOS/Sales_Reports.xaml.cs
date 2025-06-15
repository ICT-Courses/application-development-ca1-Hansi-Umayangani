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
    /// Interaction logic for Sales_Reports.xaml
    /// </summary>
    public partial class Sales_Reports : Window
    {
        public Sales_Reports()
        {
            InitializeComponent();
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

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrintBillButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SalesReportsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
