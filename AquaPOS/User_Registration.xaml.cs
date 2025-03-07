using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
    /// Interaction logic for User_Registration.xaml
    /// </summary>
    public partial class User_Registration : Window
    {
        public User_Registration()
        {
            InitializeComponent();
            DatabaseInitializer.InitializeDatabase();
        }

        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            // This is where the create user button click event is handled
        }

    }
}
