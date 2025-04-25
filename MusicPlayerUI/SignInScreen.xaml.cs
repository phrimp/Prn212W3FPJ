using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MusicPlayerEntities;
using MusicPlayerServices;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for SignInScreen.xaml
    /// </summary>
    public partial class SignInScreen : Window
    {
        private UserService _userService;
        public SignInScreen(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        public void SwitchToLogin() => SetActivePanel(true);
        public void SwitchToRegister() => SetActivePanel(false);

        private void SetActivePanel(bool isLogin)
        {
            LoginPanel.Visibility = isLogin ? Visibility.Visible : Visibility.Collapsed;
            RegisterPanel.Visibility = isLogin ? Visibility.Collapsed : Visibility.Visible;

            LoginButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLogin ? "#E0E0E0" : "#808080"));
            RegisterButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLogin ? "#808080" : "#E0E0E0"));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) => SetActivePanel(true);

        private void RegisterButton_Click(object sender, RoutedEventArgs e) => SetActivePanel(false);

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            HomeScreen homeScreen = new HomeScreen();
            homeScreen.Show();
            this.Close();
        }

        private void LoginSubmit_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsername.Text;
            string password = LoginPassword.Password;
            bool rememberMe = RememberMeCheckbox.IsChecked ?? false;

            try
            {
                var user = _userService.SignIn(username, password);

                if (user != null)
                {
                    // Store current user information for session
                    App.Current.Properties["CurrentUser"] = user;
                    App.Current.Properties["UserRole"] = user.Role?.Name ?? "User";

                    // Update last login date
                    user.LastLoginDate = DateTime.Now;
                    _userService.Update(user);

                    // Navigate to home screen
                    HomeScreen homeScreen = new HomeScreen(user);
                    homeScreen.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password. Please try again.", "Login Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterSubmit_Click(object sender, RoutedEventArgs e)
        {
            string email = RegisterEmail.Text;
            string username = RegisterUsername.Text;
            string password = RegisterPassword.Password;
            string confirmPassword = RegisterConfirmPassword.Password;
            string fullName = RegisterFullName.Text;
            string profilePicture = RegisterProfilePicture.Text;

            if (ValidateRegistration(email, username, fullName, password, confirmPassword))
            {
                try
                {
                    // Create a new user with the User role (1)
                    var newUser = new User
                    {
                        Email = email,
                        Username = username,
                        PasswordHash = password, // Will be encrypted in the repository
                        FullName = fullName,
                        ProfilePicture = profilePicture,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        RoleId = 1 // Default role is User
                    };

                    _userService.Add(newUser);

                    MessageBox.Show("Account created successfully! You can now log in.", "Registration Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    SetActivePanel(true); // Switch to login
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Registration error: {ex.Message}", "Registration Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateRegistration(string email, string username, string fullName, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Email, username, full name, and password are required.", "Registration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Registration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Registration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Registration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Password reset functionality will be implemented in a future update.",
                "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Terms_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Terms of Service document will be displayed here.",
                "Terms of Service", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Privacy_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Privacy Policy document will be displayed here.",
                "Privacy Policy", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}