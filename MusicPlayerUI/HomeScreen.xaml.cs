using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MusicPlayerEntities;
using MusicPlayerServices;

namespace MusicPlayerUI
{
    public partial class HomeScreen : Window
    {
        private UserService _userService;
        private SongService _songService;
        private int _currentUserId = 1;
        private DispatcherTimer progressTimer;

        public HomeScreen()
        {
            InitializeComponent();
            _userService = new UserService();
            _songService = new SongService();

            LoadFavorites();
            InitializeProgressTimer();

            // Set event handler for window closing to clean up resources
            this.Closing += HomeScreen_Closing;
        }

        private void InitializeProgressTimer()
        {
            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(500); // Update every 500ms
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (_songService.GetPlaybackState() == NAudio.Wave.PlaybackState.Playing)
            {
                float position = _songService.GetCurrentPosition();
                float duration = _songService.GetCurrentDuration();

                if (duration > 0)
                {
                    // Calculate percentage and update progress bar
                    double progressPercentage = (position / duration) * 100;
                    SongProgressBar.Value = progressPercentage;
                }
            }
        }

        private void HomeScreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop timer
            if (progressTimer != null)
            {
                progressTimer.Stop();
            }

            // Clean up player resources when window is closed
            _songService.Dispose();
        }

        private void LoadFavorites()
        {
            try
            {
                Favorite_List.Items.Clear();

                List<Song> favorites = _songService.GetAll();
                if (favorites == null) { return; }

                int index = 1;
                foreach (Song song in favorites)
                {
                    // Create a song view model for binding
                    var songVM = new SongViewModel
                    {
                        Index = index.ToString(),
                        Title = song.Title,
                        Artist = song.Artist?.Name ?? "Unknown Artist",
                        Album = song.Album?.Title ?? "Unknown Album",
                        Duration = FormatDuration(song.Duration),
                        SongData = song
                    };

                    Favorite_List.Items.Add(songVM);
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading favorites: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatDuration(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return $"{time.Minutes}:{time.Seconds:D2}";
        }

        private void Favorite_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = Favorite_List.SelectedItem as SongViewModel;

            if (selectedItem != null)
            {
                song_name.Text = selectedItem.Title;
                song_artist.Text = selectedItem.Artist;
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
            var selectedItem = Favorite_List.SelectedItem as SongViewModel;
            if (selectedItem != null)
            {
                _songService.PlaySong(selectedItem.SongData);
                PlayBtn.Content = "❚❚";
                UpdateNowPlayingInfo();
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
            bool isLooping = _songService.IsLooping();
            LoopBtn.Foreground = isLooping
                ? new SolidColorBrush(Color.FromRgb(29, 185, 84))
                : new SolidColorBrush(Color.FromRgb(179, 179, 179));
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

                // Reset progress bar when song changes
                SongProgressBar.Value = 0;
            }
        }
    }

    // ViewModel for song display in ListView
    public class SongViewModel
    {
        public string Index { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public Song SongData { get; set; }
    }
}