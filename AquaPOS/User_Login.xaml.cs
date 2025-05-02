using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AquaPOS;

namespace AquaPOS
{
    /// <summary>
    /// Interaction logic for MainWindo.xaml
    /// </summary>
    public partial class User_Login : Window
    {
        public User_Login()
        {
            InitializeComponent();
            DatabaseInitializer.InitializeDatabase();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // This is the function that runs when the user clicks the login button

            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both Username and Password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = DatabaseInitializer.ConnectionString;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserRole FROM Users WHERE Username=@Username AND Password=@Password";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string userRole = result.ToString();

                        Window nextWindow = null;
                        if (userRole == "Cashier")
                        {
                            nextWindow = new Sales_Processing();
                        }
                        else if (userRole == "Admin")
                        {
                            nextWindow = new Dashboard();
                        }

                        if (nextWindow != null)
                        {
                            nextWindow.Show();
                            this.Close(); // Close login window and navigate to next window
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            User_Registration userRegistrationForm = new User_Registration();
            userRegistrationForm.Show();
            this.Close();
        }

        private void ShowPasswordCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        private void ShowPasswordCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
