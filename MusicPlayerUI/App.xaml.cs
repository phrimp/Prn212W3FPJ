using MusicPlayerServices;
using System.Windows;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the services
            UserService userService = new UserService();

            // Create and show the sign-in screen
            SignInScreen signInScreen = new SignInScreen(userService);
            signInScreen.Show();
        }
    }
}