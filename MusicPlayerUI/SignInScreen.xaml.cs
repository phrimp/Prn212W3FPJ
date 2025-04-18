using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MusicPlayerServices;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for SignInScreen.xaml
    /// </summary>
    public partial class SignInScreen : Window
    {
        private IUserService _userService;
        public SignInScreen(IUserService userService)
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

            if (ValidateLogin(username, password))
            {
                // TODO: Save login if rememberMe is true
                HomeScreen homeScreen = new HomeScreen();
                homeScreen.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password. Please try again.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterSubmit_Click(object sender, RoutedEventArgs e)
        {
            string email = RegisterEmail.Text;
            string username = RegisterUsername.Text;
            string password = RegisterPassword.Password;
            string confirmPassword = RegisterConfirmPassword.Password;

            if (ValidateRegistration(email, username, password, confirmPassword))
            {
                MessageBox.Show("Account created successfully! You can now log in.", "Registration Successful",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                SetActivePanel(true); // Switch to login
            }
        }

        private bool ValidateLogin(string username, string password)
        {
            // Replace this with actual login logic
            return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
        }

        private bool ValidateRegistration(string email, string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("All fields are required.", "Registration Error",
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
