using MusicPlayerEntities;
using MusicPlayerServices;
using System;
using System.Linq;
using System.Windows;

namespace MusicPlayerUI
{
    public partial class AddAlbumWindow : Window
    {
        public string AlbumTitle { get; private set; }
        private readonly AlbumService _albumService;
        private readonly ArtistService _artistService;

        public AddAlbumWindow(AlbumService albumService, ArtistService artistService)
        {
            InitializeComponent();
            _albumService = albumService;
            LoadArtists();
            ReleaseYearTextBox.Text = DateTime.Now.Year.ToString();
            _artistService = artistService;
        }

        private void LoadArtists()
        {
            try
            {
                
                    var artists = _artistService.GetArtistsOrderByName();
                    ArtistComboBox.ItemsSource = artists;
                    ArtistComboBox.DisplayMemberPath = "Name";
                    ArtistComboBox.SelectedValuePath = "ArtistId";

                    if (artists.Count > 0)
                    {
                        ArtistComboBox.SelectedIndex = 0;
                    }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading artists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(AlbumTitleTextBox.Text))
                {
                    ErrorMessageText.Text = "Please enter an album title.";
                    return;
                }

                if (ArtistComboBox.SelectedItem == null)
                {
                    ErrorMessageText.Text = "Please select an artist.";
                    return;
                }

                int releaseYear = 0;
                if (!string.IsNullOrWhiteSpace(ReleaseYearTextBox.Text) &&
                    !int.TryParse(ReleaseYearTextBox.Text, out releaseYear))
                {
                    ErrorMessageText.Text = "Release year must be a valid number.";
                    return;
                }

                var artist = ArtistComboBox.SelectedItem as Artist;
                var newAlbum = new Album
                {
                    Title = AlbumTitleTextBox.Text,
                    ArtistId = artist.ArtistId,
                    ReleaseYear = !string.IsNullOrWhiteSpace(ReleaseYearTextBox.Text) ? (int?)releaseYear : null,
                    CoverImageUrl = CoverImageUrlTextBox.Text ?? string.Empty
                };
                _albumService.AddNewAlbum(newAlbum);


                AlbumTitle = AlbumTitleTextBox.Text;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Error adding album: {ex.Message}";
                MessageBox.Show($"Error adding album: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}