using MusicPlayerRepositories;
using MusicPlayerServices;
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
using System.IO;
using NAudio.Wave;
using Microsoft.Win32;
using MusicPlayerEntities;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for AddSongWindow.xaml
    /// </summary>
    public partial class AddSongWindow : Window
    {
        private readonly SongService _songService;
        private readonly ArtistService _artistService;
        private readonly AlbumService _albumService;
        private readonly GenreService _genreService;
        private string _selectedFilePath;
        private int _fileDuration;
        private readonly string _targetDirectory;

        public AddSongWindow(SongService songService, ArtistService artistService, AlbumService albumService, GenreService genreService)
        {
            InitializeComponent();
            _songService = songService;
            _artistService = artistService;
            _albumService = albumService;
            _genreService = genreService;

            //Songs Directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _targetDirectory = Path.Combine(baseDirectory, "..", "..", "..", "Assets", "Songs");
            Directory.CreateDirectory(_targetDirectory);

            LoadArtists();
            LoadAlbums();
            LoadGenres();

            ReleaseDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadArtists()
        {
            try
            {
                var artists = _artistService.GetAllArtists();
                ArtistComboBox.ItemsSource = artists;
                ArtistComboBox.DisplayMemberPath = "Name";
                ArtistComboBox.SelectedValuePath = "ArtistId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading artists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAlbums()
        {
            try
            {
                var albums = _albumService.GetAll();
                AlbumComboBox.ItemsSource = albums;
                AlbumComboBox.DisplayMemberPath = "Title";
                AlbumComboBox.SelectedValuePath = "AlbumId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading albums: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGenres()
        {
            try
            {
                var genres = _genreService.GetAll();
                GenreComboBox.ItemsSource = genres;
                GenreComboBox.DisplayMemberPath = "Name";
                GenreComboBox.SelectedValuePath = "GenreId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading genres: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Title = "Select MP3 File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                SelectedFileText.Text = Path.GetFileName(_selectedFilePath);

                // Get duration from the mp3 file
                try
                {
                    using (var audioFile = new AudioFileReader(_selectedFilePath))
                    {
                        _fileDuration = (int)audioFile.TotalTime.TotalSeconds;
                        DurationTextBox.Text = _fileDuration.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading audio file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _fileDuration = 0;
                    DurationTextBox.Text = "Unknown";
                }
            }
        }

        private void NewArtistButton_Click(object sender, RoutedEventArgs e)
        {
            var newArtistWindow = new AddArtistWindow(_artistService);
            if (newArtistWindow.ShowDialog() == true)
            {
                LoadArtists();

                var newArtistName = newArtistWindow.ArtistName;
                foreach (var artist in (List<Artist>)ArtistComboBox.ItemsSource)
                {
                    if (artist.Name == newArtistName)
                    {
                        ArtistComboBox.SelectedItem = artist;
                        break;
                    }
                }
            }
        }
        private void NewAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            var newAlbumWindow = new AddAlbumWindow(_albumService, _artistService);
            if (newAlbumWindow.ShowDialog() == true)
            {
                LoadAlbums();

                // Select the newly added album
                var newAlbumTitle = newAlbumWindow.AlbumTitle;
                foreach (var album in (List<Album>)AlbumComboBox.ItemsSource)
                {
                    if (album.Title == newAlbumTitle)
                    {
                        AlbumComboBox.SelectedItem = album;
                        break;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    ErrorMessageText.Text = "Please enter a song title.";
                    return;
                }

                if (ArtistComboBox.SelectedItem == null && string.IsNullOrWhiteSpace(ArtistComboBox.Text))
                {
                    ErrorMessageText.Text = "Please select or enter an artist.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(_selectedFilePath))
                {
                    ErrorMessageText.Text = "Please select an MP3 file.";
                    return;
                }

                int artistId;
                var selectedArtist = ArtistComboBox.SelectedItem as Artist;

                if (selectedArtist != null)
                {
                    artistId = selectedArtist.ArtistId;
                }
                else
                {
                    var newArtist = new Artist
                    {
                        Name = ArtistComboBox.Text,
                        Bio = string.Empty
                    };

                    _artistService.AddNewArtist(newArtist);
                    artistId = newArtist.ArtistId;
                }

                int? albumId = null;
                var selectedAlbum = AlbumComboBox.SelectedItem as Album;

                if (selectedAlbum != null)
                {
                    albumId = selectedAlbum.AlbumId;
                }
                else if (!string.IsNullOrWhiteSpace(AlbumComboBox.Text))
                {
                    var newAlbum = new Album
                    {
                        Title = AlbumComboBox.Text,
                        ArtistId = artistId,
                        ReleaseYear = DateTime.Now.Year
                    };
                    _albumService.AddNewAlbum(newAlbum);
                    albumId = newAlbum.AlbumId;
                }

                int? genreId = null;
                var selectedGenre = GenreComboBox.SelectedItem as Genre;

                if (selectedGenre != null)
                {
                    genreId = selectedGenre.GenreId;
                }

                string uniqueFileName = $"{Guid.NewGuid()}.mp3";
                string targetFilePath = Path.Combine(_targetDirectory, uniqueFileName);

                File.Copy(_selectedFilePath, targetFilePath, true);

                var newSong = new Song
                {
                    Title = TitleTextBox.Text,
                    ArtistId = artistId,
                    AlbumId = albumId,
                    GenreId = genreId,
                    Duration = _fileDuration,
                    FilePath = uniqueFileName,
                    ReleaseDate = ReleaseDatePicker.SelectedDate.HasValue
                        ? DateOnly.FromDateTime(ReleaseDatePicker.SelectedDate.Value)
                        : null,
                    PlayCount = 0
                };

                _songService.Add(newSong);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Error saving song: {ex.Message}";
                MessageBox.Show($"Error saving song: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
