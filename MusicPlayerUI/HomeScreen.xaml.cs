using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MusicPlayerEntities;
using MusicPlayerServices;

namespace MusicPlayerUI
{
    public partial class HomeScreen : Window
    {
        private UserService _userService;
        private SongService _songService;
        private int _currentUserId = 1;

        public HomeScreen()
        {
            InitializeComponent();
            _userService = new UserService();
            _songService = new SongService();

            LoadFavorites();

            // Set event handler for window closing to clean up resources
            this.Closing += HomeScreen_Closing;
        }

        private void HomeScreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Clean up player resources when window is closed
            _songService.Dispose();
        }

        private void LoadFavorites()
        {
            try
            {
                Favorite_List.Items.Clear();

                List<Song> favotites = _songService.GetAll();
                if (favotites == null) { return; }

                int index = 1;
                foreach (Song s in favotites)
                {
                    Favorite_List.Items.Add(CreateSongListItem(s, index));
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading favorites: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ListViewItem CreateSongListItem(Song song, int index)
        {
            ListViewItem item = new ListViewItem();

            item.Tag = song;

            Grid grid = new Grid();
            grid.Width = 620;

            ColumnDefinition col1 = new ColumnDefinition() { Width = new GridLength(40) };
            ColumnDefinition col2 = new ColumnDefinition() { Width = new GridLength(200) };
            ColumnDefinition col3 = new ColumnDefinition() { Width = new GridLength(150) };
            ColumnDefinition col4 = new ColumnDefinition() { Width = new GridLength(150) };
            ColumnDefinition col5 = new ColumnDefinition() { Width = new GridLength(80) };

            grid.ColumnDefinitions.Add(col1);
            grid.ColumnDefinitions.Add(col2);
            grid.ColumnDefinitions.Add(col3);
            grid.ColumnDefinitions.Add(col4);
            grid.ColumnDefinitions.Add(col5);

            TextBlock indexTB = new TextBlock() { Text = index.ToString(), VerticalAlignment = VerticalAlignment.Center };
            TextBlock titleTB = new TextBlock() { Text = song.Title, VerticalAlignment = VerticalAlignment.Center };

            TextBlock artistTB = new TextBlock()
            {
                Text = song.Artist?.Name ?? "Unknown Artist",
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock albumTB = new TextBlock()
            {
                Text = song.Album?.Title ?? "Unknown Album",
                VerticalAlignment = VerticalAlignment.Center
            };

            TimeSpan time = TimeSpan.FromSeconds(song.Duration);
            string formattedDuration = $"{time.Minutes}:{time.Seconds:D2}";
            TextBlock durationTB = new TextBlock()
            {
                Text = formattedDuration,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(indexTB, 0);
            Grid.SetColumn(titleTB, 1);
            Grid.SetColumn(artistTB, 2);
            Grid.SetColumn(albumTB, 3);
            Grid.SetColumn(durationTB, 4);

            grid.Children.Add(indexTB);
            grid.Children.Add(titleTB);
            grid.Children.Add(artistTB);
            grid.Children.Add(albumTB);
            grid.Children.Add(durationTB);

            item.Content = grid;

            item.Selected += (sender, e) => {
                song_name.Text = song.Title;
                song_artist.Text = song.Artist?.Name ?? "Unknown Artist";
            };

            // Double-click to play the song directly
            item.MouseDoubleClick += (sender, e) => {
                PlaySelectedSong();
            };

            return item;
        }

        private void Favorite_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selectedItem = Favorite_List.SelectedItem as ListViewItem;

            if (selectedItem != null)
            {
                Song selectedSong = selectedItem.Tag as Song;

                if (selectedSong != null)
                {
                    song_name.Text = selectedSong.Title;
                    song_artist.Text = selectedSong.Artist?.Name ?? "Unknown Artist";

                    selectedItem.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333
                }
            }
        }

        private void PlaySong(object sender, RoutedEventArgs e)
        {
            if (_songService.IsPaused())
            {
                // If paused, resume playback
                _songService.ResumePlayback();
                PlayBtn.Content = "❚❚";
            }
            else if (_songService.GetPlaybackState() == NAudio.Wave.PlaybackState.Playing)
            {
                // If playing, pause playback
                _songService.PausePlayback();
                PlayBtn.Content = "▶";
            }
            else
            {
                // If stopped or no current song, play the selected song or queue
                if (Favorite_List.SelectedItem != null)
                {
                    PlaySelectedSong();
                }
                else
                {
                    // If no song is selected but there are songs in favorites, load all favorites to queue
                    LoadFavoritesToQueue();
                    _songService.PlayFromQueue();
                    UpdateNowPlayingInfo();
                }
            }
        }

        private void PlaySelectedSong()
        {
            ListViewItem selectedItem = Favorite_List.SelectedItem as ListViewItem;
            if (selectedItem != null)
            {
                Song selectedSong = selectedItem.Tag as Song;
                if (selectedSong != null)
                {
                    _songService.PlaySong(selectedSong);
                    PlayBtn.Content = "❚❚";
                    UpdateNowPlayingInfo();
                }
            }
        }

        private void LoadFavoritesToQueue()
        {
            // First clear the current queue
            _songService.ClearQueue();

            // Add all favorite songs to queue
            List<Song> favorites = _userService.GetMyFavotites(_currentUserId);
            if (favorites != null && favorites.Count > 0)
            {
                foreach (var song in favorites)
                {
                    _songService.AddToQueue(song);
                }
            }
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {
            _songService.NextSong();
            UpdateNowPlayingInfo();
            PlayBtn.Content = "❚❚";
        }

        private void PreviousSong_Click(object sender, RoutedEventArgs e)
        {
            _songService.PreviousSong();
            UpdateNowPlayingInfo();
            PlayBtn.Content = "❚❚";
        }

        private void ToggleLoop_Click(object sender, RoutedEventArgs e)
        {
            _songService.ToggleLoop();
            // Update UI to reflect loop state
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_songService != null)
            {
                float volume = (float)(e.NewValue / 100.0); // Convert from percentage to 0-1 range
                _songService.SetVolume(volume);
            }
        }

        private void UpdateNowPlayingInfo()
        {
            Song currentSong = _songService.GetCurrentSong();
            if (currentSong != null)
            {
                song_name.Text = currentSong.Title;
                song_artist.Text = currentSong.Artist?.Name ?? "Unknown Artist";
            }
        }
    }
}