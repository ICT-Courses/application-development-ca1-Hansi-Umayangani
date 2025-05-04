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
    /// Interaction logic for Sales_Processing.xaml
    /// </summary>
    public partial class Sales_Processing : Window
    {
        public Sales_Processing()
        {
            InitializeComponent();
        }

        private void ProductIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            User_Login user_Login = new User_Login();
            user_Login.Show(); // Show the login window
            this.Hide(); // Close the current window
        }

        private void SearchProduct_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void txtSearchProduct_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void txtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void txtProductNameResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtCategoryResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtUnitPriceResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtAvailableQtyResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
