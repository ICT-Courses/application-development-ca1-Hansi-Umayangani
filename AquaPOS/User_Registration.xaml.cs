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

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string reenterPassword = ReenterPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username cannot be empty.");
                return;
            }

            if (UserRoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a user role.");
                return;
            }

            string userRole = (UserRoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (password != reenterPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            string connectionString = DatabaseInitializer.ConnectionString;
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Check if the username already exists
                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username=@Username";
                using (var checkCommand = new SQLiteCommand(checkUserQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Username", username);
                    var userExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userExists > 0)
                    {
                        MessageBox.Show("Username already exists.");
                        return;
                    }
                }

                // Insert the new user into the database
                string insertQuery = "INSERT INTO Users (Username, Password, UserRole) VALUES (@Username, @Password, @UserRole)";
                using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Username", username);
                    insertCommand.Parameters.AddWithValue("@Password", password);
                    insertCommand.Parameters.AddWithValue("@UserRole", userRole);

                    insertCommand.ExecuteNonQuery();
                    MessageBox.Show("User created successfully.");

                    // Close current window and navigate to the login screen
                    this.Hide();
                    User_Login loginForm = new User_Login();
                    loginForm.Show();
                }
            }
        }
    }
}
