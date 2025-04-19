using MusicPlayerEntities;
using MusicPlayerServices;
using System;
using System.Windows;

namespace MusicPlayerUI
{
    public partial class AddArtistWindow : Window
    {
        public string ArtistName { get; private set; }
        private readonly ArtistService _artistService;
        public AddArtistWindow(ArtistService artistService)
        {
            InitializeComponent();
            _artistService = artistService;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(ArtistNameTextBox.Text))
                {
                    ErrorMessageText.Text = "Please enter an artist name.";
                    return;
                }

                    var newArtist = new Artist
                    {
                        Name = ArtistNameTextBox.Text,
                        Bio = BioTextBox.Text ?? string.Empty,
                        ImageUrl = ImageUrlTextBox.Text ?? string.Empty
                    };
                
                _artistService.AddNewArtist(newArtist);
                ArtistName = ArtistNameTextBox.Text;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Error adding artist: {ex.Message}";
                MessageBox.Show($"Error adding artist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}