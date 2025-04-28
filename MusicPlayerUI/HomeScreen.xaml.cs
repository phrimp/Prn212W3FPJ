using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MusicPlayerEntities;
using MusicPlayerRepositories;
using MusicPlayerServices;
using System.Windows.Input; // Add this to fix the MouseButtonEventArgs error

namespace MusicPlayerUI
{
    public partial class HomeScreen : Window
    {
        private UserService _userService;
        private SongService _songService;
        private ArtistService _artistService;
        private AlbumService _albumService;
        private GenreService _genreService;
        private int _currentUserId = 1;
        private DispatcherTimer progressTimer;
        private Artist _selectedArtist;
        private User _currentUser;
        private bool _userIsDraggingSlider = false;
        private bool _wasPlayingBeforeDrag = false;
        private List<Song> _currentPlaylistSongs = new List<Song>();
        private int _currentPlayingIndex = -1;
        private readonly PlaylistRepository _playlistRepo = PlaylistRepository.Instance;
        private int _currentPlaylistId;




        public HomeScreen(User currentUser = null, int playlistId = 0)
        {
            InitializeComponent();
            _userService = new UserService();
            _songService = new SongService();
            _artistService = new ArtistService();
            _albumService = new AlbumService();
            _genreService = new GenreService();
            _playlistRepo = new PlaylistRepository();
            _currentPlaylistId = playlistId;

            if (_currentPlaylistId > 0)
            {
                PlaylistView.Visibility = Visibility.Visible;
                LoadPlaylist(_currentPlaylistId);
            }

            if (currentUser != null)
            {
                _currentUser = currentUser;
                _currentUserId = currentUser.UserId;

                ConfigureUIForUserRole(currentUser.RoleId);
            }
            else
            {
                ConfigureUIForGuestMode();
            }

            InitializeHomeView();

            LoadFavorites();
            LoadUserPlaylists();
            SetupProgressTimer();

            this.Closing += HomeScreen_Closing;
        }

        private void ConfigureUIForUserRole(int roleId)
        {
            switch (roleId)
            {
                case 3: // Admin
                    ConfigureUIForAdminRole();
                    break;
                case 2: // Artist
                    ConfigureUIForArtistRole();
                    break;
                case 1: // User
                default:
                    ConfigureUIForUserRole();
                    break;
            }
        }

        private void ConfigureUIForAdminRole()
        {
            UserNameText.Text = _currentUser.Username;
            RoleText.Text = "Admin";

            AdminPanel.Visibility = Visibility.Visible;

            AddSongButton.IsEnabled = true;
            AddArtistButton.IsEnabled = true;
            AddAlbumButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            ManageUsersButton.IsEnabled = true;
        }

        private void ConfigureUIForArtistRole()
        {
            // Artists can manage their own content
            UserNameText.Text = _currentUser.Username;
            RoleText.Text = "Artist";

            // Enable artist-specific controls
            AdminPanel.Visibility = Visibility.Collapsed;

            // Configure permissions
            AddSongButton.IsEnabled = true;
            AddArtistButton.IsEnabled = false;
            AddAlbumButton.IsEnabled = true;
            DeleteButton.IsEnabled = false;
            ManageUsersButton.IsEnabled = false;

            // Load only the artist's own content
            LoadArtistSpecificContent();
        }

        private void ConfigureUIForUserRole()
        {
            // Regular users have basic access
            UserNameText.Text = _currentUser.Username;
            RoleText.Text = "User";

            // Configure UI for standard user
            AdminPanel.Visibility = Visibility.Collapsed;

            // Disable management controls
            AddSongButton.IsEnabled = false;
            AddArtistButton.IsEnabled = false;
            AddAlbumButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            ManageUsersButton.IsEnabled = false;
        }

        private void ConfigureUIForGuestMode()
        {
            UserNameText.Text = "Guest";
            RoleText.Text = "Guest";

            AdminPanel.Visibility = Visibility.Collapsed;

            AddSongButton.IsEnabled = false;
            AddArtistButton.IsEnabled = false;
            AddAlbumButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            ManageUsersButton.IsEnabled = false;

            CreatePlaylistButton.IsEnabled = false;
            //FavoriteButton.IsEnabled = false;
        }

        private void LoadArtistSpecificContent()
        {
            try
            {
                var artistRepository = ArtistRepository.Instance;
                var userArtist = artistRepository.GetAllArtists()
                    .FirstOrDefault(a => a.Name.Equals(_currentUser.Username, StringComparison.OrdinalIgnoreCase));

                if (userArtist != null)
                {
                    int artistId = userArtist.ArtistId;

                    ShowArtistDetails(userArtist);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading artist content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region View Navigation

        private void ShowView(UIElement viewToShow)
        {
            // Reset all navigation button backgrounds
            HomeButton.Background = new SolidColorBrush(Colors.Transparent);
            MyMusicButton.Background = new SolidColorBrush(Colors.Transparent);
            FavoritesButton.Background = new SolidColorBrush(Colors.Transparent);

            // Hide all views
            HomeView.Visibility = Visibility.Collapsed;
            MyMusicView.Visibility = Visibility.Collapsed;
            FavoritesView.Visibility = Visibility.Collapsed;
            PlaylistView.Visibility = Visibility.Collapsed;
            ArtistDetailView.Visibility = Visibility.Collapsed;

            // Show the requested view
            viewToShow.Visibility = Visibility.Visible;
        }


        private void MyMusicButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(MyMusicView);
            MyMusicButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));

            // Load artists if we haven't already
            if (ArtistsPanel.Children.Count == 0)
            {
                LoadArtists();
            }
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(FavoritesView);
            FavoritesButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
        }

        private void ShowArtistDetails(Artist artist)
        {
            _selectedArtist = artist;
            ShowView(ArtistDetailView);

            // Set artist details
            ArtistNameText.Text = artist.Name;
            ArtistBioText.Text = string.IsNullOrEmpty(artist.Bio) ?
                "No biography available for this artist." : artist.Bio;

            // Set artist initial for the avatar
            if (!string.IsNullOrEmpty(artist.Name))
            {
                ArtistInitial.Text = artist.Name[0].ToString().ToUpper();
            }
            else
            {
                ArtistInitial.Text = "A";
            }

            // Load artist's songs
            LoadArtistSongs(artist.ArtistId);
        }

        private void PlaylistsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPlaylist = PlaylistsListBox.SelectedItem as ListBoxItem;
            if (selectedPlaylist != null)
            {
                // Get the playlist ID from the Tag property
                if (int.TryParse(selectedPlaylist.Tag.ToString(), out int playlistId))
                {
                    LoadPlaylist(playlistId);
                    ShowView(PlaylistView);
                }
            }
        }

        #endregion

        #region Progress Timer

        private void SetupProgressTimer()
        {
            progressTimer = new DispatcherTimer();
            progressTimer.Interval = TimeSpan.FromMilliseconds(100); // Update 10 times per second
            progressTimer.Tick += (s, e) =>
            {
                if (!_userIsDraggingSlider && _songService.IsPlaying())
                {
                    double currentPosition = _songService.GetPlaybackPosition();
                    double totalDuration = _songService.GetCurrentSongDuration();

                    if (totalDuration > 0)
                    {
                        // Update slider
                        double progressPercentage = (currentPosition / totalDuration) * 100;
                        SongProgressSlider.Value = progressPercentage;

                        // Update time display
                        CurrentTimeText.Text = FormatTimeSpan(TimeSpan.FromSeconds(currentPosition));
                        TotalTimeText.Text = FormatTimeSpan(TimeSpan.FromSeconds(totalDuration));
                    }
                }
            };
            progressTimer.Start();
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

        #endregion

        #region Favorites

        private void LoadFavorites()
        {
            try
            {
                Favorite_List.Items.Clear();

                List<Song> favorites = _songService.GetFavoriteSongsByUserId(_currentUserId);
                if (favorites == null || favorites.Count == 0)
                {
                    return;
                }

                int index = 1;
                foreach (Song song in favorites)
                {
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

        private void Favorite_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = Favorite_List.SelectedItem as SongViewModel;

            if (selectedItem != null)
            {
                song_name.Text = selectedItem.Title;
                song_artist.Text = selectedItem.Artist;
            }
        }

        #endregion

        #region Artists

        private void LoadArtists()
        {
            try
            {
                ArtistsPanel.Children.Clear();

                // Add diagnostic logging
                Console.WriteLine("Attempting to load artists...");
                List<Artist> artists = _artistService.GetAllArtists();
                Console.WriteLine($"Retrieved {artists?.Count ?? 0} artists from service");

                if (artists == null || artists.Count == 0)
                {
                    MessageBox.Show("No artists found in the database.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (var artist in artists)
                {
                    Border artistCard = CreateArtistCard(artist);
                    ArtistsPanel.Children.Add(artistCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading artists: {ex.Message}\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateArtistCard(Artist artist)
        {
            // Main container
            Border card = new Border
            {
                Width = 180,
                Height = 220,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525")),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10)
            };

            // Add a mouse click event to show artist details
            card.MouseLeftButtonUp += (s, e) => ShowArtistDetails(artist);

            // Card content
            StackPanel content = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Artist Avatar/Circle
            Border avatarBorder = new Border
            {
                Width = 150,
                Height = 150,
                CornerRadius = new CornerRadius(75), // Makes it a circle
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Initial letter
            TextBlock initial = new TextBlock
            {
                Text = !string.IsNullOrEmpty(artist.Name) ? artist.Name[0].ToString().ToUpper() : "A",
                FontSize = 70,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            avatarBorder.Child = initial;
            content.Children.Add(avatarBorder);

            // Artist Name
            TextBlock nameText = new TextBlock
            {
                Text = artist.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.Children.Add(nameText);

            card.Child = content;
            return card;
        }



        private void LoadArtistSongs(int artistId)
        {
            try
            {
                ArtistSongs_List.Items.Clear();
                List<Song> songs = _songService.GetSongsByArtist(artistId);

                // Update artist info text
                int albumCount = CountUniqueAlbums(songs);
                ArtistInfoText.Text = $"{songs.Count} songs • {albumCount} albums";

                int index = 1;
                foreach (Song song in songs)
                {
                    // Create a song view model for binding
                    var songVM = new SongViewModel
                    {
                        Index = index.ToString(),
                        Title = song.Title,
                        Artist = song.Artist?.Name ?? "Unknown Artist",
                        Album = song.Album?.Title ?? "Unknown Album",
                        Duration = FormatDuration(song.Duration),
                        PlayCount = song.PlayCount?.ToString() ?? "0",
                        SongData = song
                    };

                    ArtistSongs_List.Items.Add(songVM);
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading artist songs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CountUniqueAlbums(List<Song> songs)
        {
            HashSet<int?> uniqueAlbumIds = new HashSet<int?>();
            foreach (var song in songs)
            {
                if (song.AlbumId.HasValue)
                {
                    uniqueAlbumIds.Add(song.AlbumId);
                }
            }
            return uniqueAlbumIds.Count;
        }

        private void ArtistSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // We'll implement the search when the Search button is clicked
        }

        private void SearchArtists_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = ArtistSearchBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LoadArtists(); // Load all artists
                return;
            }

            try
            {
                ArtistsPanel.Children.Clear();
                List<Artist> allArtists = _artistService.GetAllArtists();

                // Filter artists by name
                var filteredArtists = allArtists.FindAll(a =>
                    a.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);

                foreach (var artist in filteredArtists)
                {
                    // Create an artist card
                    Border artistCard = CreateArtistCard(artist);
                    ArtistsPanel.Children.Add(artistCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching artists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ArtistSongs_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ArtistSongs_List.SelectedItem as SongViewModel;

            if (selectedItem != null)
            {
                song_name.Text = selectedItem.Title;
                song_artist.Text = selectedItem.Artist;
            }
        }

        private void PlayAllArtistSongs_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedArtist != null)
            {
                try
                {
                    // Use the service to play all songs by the selected artist
                    _songService.PlayAllSongsByArtist(_selectedArtist.ArtistId);

                    // Update UI
                    PlayBtn.Content = "❚❚";
                    UpdateNowPlayingInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error playing artist songs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Playlists

        private void LoadPlaylist(int playlistId)
        {
            try
            {
                // Get the repository through dependency injection if possible
                //var playlistRepository = PlaylistRepository.Instance;
                //var playlist = playlistRepository.GetOne(playlistId);

                var playlist = _playlistRepo.GetOne(playlistId);

                if (playlist == null)
                {
                    MessageBox.Show("Playlist not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Cập nhật lại _currentPlaylistId để chắc chắn
                _currentPlaylistId = playlist.PlaylistId;

                SelectedPlaylistNameText.Text = playlist.Name;

                Playlist_Songs_List.Items.Clear();
                var songs = _playlistRepo.GetSongsFromPlaylist(_currentPlaylistId);
                _currentPlaylistSongs = songs;


                int totalSeconds = songs.Sum(s => s.Duration);
                var totalDuration = TimeSpan.FromSeconds(totalSeconds);
                SelectedPlaylistInfoText.Text = $"{songs.Count} songs • {FormatTotalDuration(totalDuration)}";

                int index = 1;
                foreach (Song song in songs)
                {
                    var songVM = new SongViewModel
                    {
                        Index = index.ToString(),
                        Title = song.Title,
                        Artist = song.Artist?.Name ?? "Unknown Artist",
                        Album = song.Album?.Title ?? "Unknown Album",
                        Duration = FormatDuration(song.Duration),
                        SongData = song
                    };

                    Playlist_Songs_List.Items.Add(songVM);
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Playlist_Songs_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = Playlist_Songs_List.SelectedItem as SongViewModel;

            if (selectedItem != null)
            {
                song_name.Text = selectedItem.Title;
                song_artist.Text = selectedItem.Artist;

                _currentPlayingIndex = Playlist_Songs_List.Items.IndexOf(selectedItem);
            }
        }

        private void LoadUserPlaylists()
        {
            // Clear danh sách cũ
            PlaylistsListBox.Items.Clear();

            try
            {
                // Lấy playlist của user hiện tại
                var playlists = PlaylistRepository.Instance.GetUserPlaylist(_currentUserId);

                foreach (var pl in playlists)
                {
                    var item = new ListBoxItem
                    {
                        Content = pl.Name,
                        Tag = pl.PlaylistId,
                        Padding = new Thickness(30, 8, 0, 8),
                        Foreground = Brushes.White,
                        Background = Brushes.Transparent
                    };
                    PlaylistsListBox.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi load playlist: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        #endregion

        #region Playback
        private void PlaySongAtIndex(int index)
        {
            if (_currentPlaylistSongs == null || !_currentPlaylistSongs.Any())
                return;

            // wrap-around
            if (index < 0)
                index = _currentPlaylistSongs.Count - 1;
            else if (index >= _currentPlaylistSongs.Count)
                index = 0;

            _currentPlayingIndex = index;
            var song = _currentPlaylistSongs[_currentPlayingIndex];

            // chọn lại item trong ListBox để đồng bộ UI
            Playlist_Songs_List.SelectedIndex = _currentPlayingIndex;
            Playlist_Songs_List.ScrollIntoView(Playlist_Songs_List.SelectedItem);

            // phát bài
            _songService.PlaySong(song);
            PlayBtn.Content = "❚❚";

            // cập nhật thông tin now playing
            song_name.Text = song.Title;
            song_artist.Text = song.Artist?.Name ?? "Unknown Artist";
            UpdateNowPlayingInfo();
        }

        private void PlaySong(object sender, RoutedEventArgs e)
        {
            if (_songService.IsPaused())
            {
                _songService.ResumePlayback();
                PlayBtn.Content = "❚❚";
                return;
            }
            if (_songService.GetPlaybackState() == NAudio.Wave.PlaybackState.Playing)
            {
                _songService.PausePlayback();
                PlayBtn.Content = "▶";
                return;
            }

            // Nếu đang ở playlist và có item được chọn
            if (GetVisibleView() == PlaylistView && Playlist_Songs_List.SelectedItem is SongViewModel vm)
            {
                int idx = Playlist_Songs_List.Items.IndexOf(vm);
                PlaySongAtIndex(idx);
            }
            else
            {
                // các view khác hoặc queue
                UIElement currentView = GetVisibleView();
                if (currentView == FavoritesView && Favorite_List.SelectedItem is SongViewModel fav)
                {
                    _songService.PlaySong(fav.SongData);
                }
                else if (currentView == ArtistDetailView && ArtistSongs_List.SelectedItem is SongViewModel art)
                {
                    _songService.PlaySong(art.SongData);
                }
                else
                {
                    _songService.PlayFromQueue();
                }
                PlayBtn.Content = "❚❚";
                UpdateNowPlayingInfo();
            }
        }

        private UIElement GetVisibleView()
        {
            if (FavoritesView.Visibility == Visibility.Visible) return FavoritesView;
            if (PlaylistView.Visibility == Visibility.Visible) return PlaylistView;
            if (ArtistDetailView.Visibility == Visibility.Visible) return ArtistDetailView;
            if (MyMusicView.Visibility == Visibility.Visible) return MyMusicView;
            if (HomeView.Visibility == Visibility.Visible) return HomeView;
            return FavoritesView; // Default to FavoritesView if nothing is visible
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {
            //_songService.NextSong();
            //UpdateNowPlayingInfo();
            //PlayBtn.Content = "❚❚";

            if (_currentPlaylistSongs == null || !_currentPlaylistSongs.Any())
                return;

            PlaySongAtIndex(_currentPlayingIndex + 1);
        }

        private void PreviousSong_Click(object sender, RoutedEventArgs e)
        {
            //_songService.PreviousSong();
            //UpdateNowPlayingInfo();
            //PlayBtn.Content = "❚❚";

            if (_currentPlaylistSongs == null || !_currentPlaylistSongs.Any())
                return;

            PlaySongAtIndex(_currentPlayingIndex - 1);
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

                // Reset progress slider when song changes
                SongProgressSlider.Value = 0;  // Changed from SongProgressBar to SongProgressSlider
                UpdateNowPlayingFavoriteButton();
            }
        }
        private void UpdateNowPlayingFavoriteButton()
        {
            if (_currentUser == null)
            {
                NowPlayingFavoriteButton.Visibility = Visibility.Collapsed;
                return;
            }

            Song currentSong = _songService.GetCurrentSong();
            if (currentSong != null)
            {
                bool isFavorite = _songService.IsFavorite(_currentUser.UserId, currentSong.SongId);

                if (isFavorite)
                {
                    NowPlayingFavoriteButton.Foreground = new SolidColorBrush(Color.FromRgb(29, 185, 84)); // Green
                }
                else
                {
                    NowPlayingFavoriteButton.Foreground = new SolidColorBrush(Colors.Gray); // Gray if not favorite
                }
            }
            else
            {
                NowPlayingFavoriteButton.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        #endregion

        #region Utilities

        private string FormatDuration(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return $"{time.Minutes}:{time.Seconds:D2}";
        }

        private string FormatTotalDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{Math.Floor(timeSpan.TotalHours)} hours {timeSpan.Minutes} minutes";
            }
            else
            {
                return $"{timeSpan.Minutes} minutes";
            }
        }

        #endregion

        #region Hand Gesture Control
        private HandGestureDetector _gestureDetector;
        private bool _gestureControlActive = false;

        /// <summary>
        /// Initializes the hand gesture detector system
        /// </summary>
        private void InitializeGestureControl()
        {
            try
            {
                _gestureDetector = new HandGestureDetector();
                _gestureDetector.GestureDetected += OnGestureDetected;
                _gestureDetector.FrameUpdated += OnFrameUpdated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing gesture control: {ex.Message}",
                    "Gesture Control Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the gesture control button click event
        /// </summary>
        private void GestureControlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_gestureDetector == null)
                {
                    InitializeGestureControl();
                }

                if (!_gestureControlActive)
                {
                    // Start gesture control
                    _gestureControlActive = true;
                    _gestureDetector?.Start();
                    GestureControlButton.Foreground = new SolidColorBrush(Color.FromRgb(29, 185, 84)); // Green
                    CameraPopup.IsOpen = true;
                    GestureStatusText.Text = "Waiting for gesture...";

                    // Update UI based on camera availability
                    if (_gestureDetector._capture == null || !_gestureDetector._capture.IsOpened())
                    {
                        GestureStatusText.Text = "Simulation mode - no camera";
                        CameraPreviewImage.Opacity = 0.8;
                    }
                    else
                    {
                        CameraPreviewImage.Opacity = 1.0;
                    }
                }
                else
                {
                    // Stop gesture control
                    StopGestureControl();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with gesture control: {ex.Message}",
                    "Gesture Control Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stops the gesture control system
        /// </summary>
        private void StopGestureControl()
        {
            _gestureControlActive = false;
            _gestureDetector?.Stop();
            GestureControlButton.Foreground = new SolidColorBrush(Colors.Gray);
            CameraPopup.IsOpen = false;
        }

        /// <summary>
        /// Handles the close button click in the camera popup
        /// </summary>
        private void CloseGestureControl_Click(object sender, RoutedEventArgs e)
        {
            StopGestureControl();
        }

        /// <summary>
        /// Updates the camera preview image
        /// </summary>
        private void OnFrameUpdated(object sender, System.Windows.Media.Imaging.WriteableBitmap frame)
        {
            CameraPreviewImage.Source = frame;
        }

        /// <summary>
        /// Handles detected gestures and performs corresponding actions
        /// </summary>
        private void OnGestureDetected(object sender, string gesture)
        {
            try
            {
                // Update status text
                GestureStatusText.Text = $"Detected: {gesture}";

                // Perform actions based on gesture
                switch (gesture.ToLower())
                {
                    case "next":
                        // Play next song
                        NextSong_Click(this, new RoutedEventArgs());
                        break;

                    case "previous":
                        // Play previous song
                        PreviousSong_Click(this, new RoutedEventArgs());
                        break;

                    case "playpause":
                        // Toggle play/pause
                        PlaySong(this, new RoutedEventArgs());
                        break;

                    case "volumeup":
                        // Increase volume
                        VolumeSlider.Value = Math.Min(VolumeSlider.Value + 10, 100);
                        break;

                    case "volumedown":
                        // Decrease volume
                        VolumeSlider.Value = Math.Max(VolumeSlider.Value - 10, 0);
                        break;

                    case "random":
                        // Play random song
                        PlayRandomSong();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling gesture: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a random song from the current playlist or library
        /// </summary>
        private void PlayRandomSong()
        {
            try
            {
                // Get current content
                List<Song> songsToChooseFrom = new List<Song>();

                // Determine which view is active and get songs accordingly
                if (PlaylistView.Visibility == Visibility.Visible && _currentPlaylistSongs != null && _currentPlaylistSongs.Any())
                {
                    songsToChooseFrom = _currentPlaylistSongs;
                }
                else if (FavoritesView.Visibility == Visibility.Visible && Favorite_List.Items.Count > 0)
                {
                    // Extract songs from favorites list
                    foreach (var item in Favorite_List.Items)
                    {
                        if (item is SongViewModel vm && vm.SongData != null)
                        {
                            songsToChooseFrom.Add(vm.SongData);
                        }
                    }
                }
                else if (ArtistDetailView.Visibility == Visibility.Visible && ArtistSongs_List.Items.Count > 0)
                {
                    // Extract songs from artist view
                    foreach (var item in ArtistSongs_List.Items)
                    {
                        if (item is SongViewModel vm && vm.SongData != null)
                        {
                            songsToChooseFrom.Add(vm.SongData);
                        }
                    }
                }
                else
                {
                    // Default: Get all songs from the library
                    songsToChooseFrom = _songService.GetAll();
                }

                // Select and play a random song
                if (songsToChooseFrom.Any())
                {
                    Random random = new Random();
                    int randomIndex = random.Next(songsToChooseFrom.Count);
                    Song randomSong = songsToChooseFrom[randomIndex];

                    // Play the song
                    _songService.PlaySong(randomSong);
                    UpdateNowPlayingInfo();
                    PlayBtn.Content = "❚❚";

                    // Update status
                    GestureStatusText.Text = $"Playing random: {randomSong.Title}";
                }
                else
                {
                    GestureStatusText.Text = "No songs available to play randomly";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing random song: {ex.Message}");
                GestureStatusText.Text = "Error playing random song";
            }
        }

        /// <summary>
        /// Cleanup resources when window is closing
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Dispose gesture detector
            if (_gestureDetector != null)
            {
                _gestureDetector.Stop();
                _gestureDetector.Dispose();
                _gestureDetector = null;
            }
        }

        #endregion

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            // Pass all the required parameters including user role, username and userService
            var addSongWindow = new AddSongWindow(
                _songService,
                _artistService,
                _albumService,
                _genreService,
                _currentUser.RoleId,       // User role
                _currentUser.Username,     // Username
                _userService               // UserService
            );

            if (addSongWindow.ShowDialog() == true)
            {
                // Refresh the current view
                LoadFavorites();

                // Show a success message
                MessageBox.Show("Song added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddSongToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            // 1) Lấy playlist hiện tại
            if (!(PlaylistsListBox.SelectedItem is ListBoxItem plItem) ||
                !int.TryParse(plItem.Tag.ToString(), out int playlistId))
            {
                MessageBox.Show("Please select a playlist in the left panel first.",
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2) Lấy danh sách tất cả bài (bạn có thể lọc theo user nếu muốn)
            List<Song> allSongs = _songService.GetAll();

            // 3) Hiện dialog chọn bài
            var dlg = new SelectSongWindow(allSongs)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() != true)
                return;

            int songId = dlg.SelectedSongId;

            try
            {
                // 4) Gọi repo thêm bài vào playlist
                PlaylistRepository.Instance.AddSongToPlaylist(songId, playlistId);

                // 5) Reload lại detail của playlist
                LoadPlaylist(playlistId);

                MessageBox.Show("Song added to playlist!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error adding song: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylistSongs == null || !_currentPlaylistSongs.Any())
            {
                MessageBox.Show("Chưa có playlist nào để trộn.", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 1) Shuffle _currentPlaylistSongs dùng Fisher–Yates
            var rng = new Random();
            for (int i = _currentPlaylistSongs.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = _currentPlaylistSongs[i];
                _currentPlaylistSongs[i] = _currentPlaylistSongs[j];
                _currentPlaylistSongs[j] = tmp;
            }

            // 2) Clear và rebuild ListBox
            Playlist_Songs_List.Items.Clear();
            int idx = 1;
            foreach (var song in _currentPlaylistSongs)
            {
                var vm = new SongViewModel
                {
                    Index = idx.ToString(),
                    Title = song.Title,
                    Artist = song.Artist?.Name ?? "Unknown Artist",
                    Album = song.Album?.Title ?? "Unknown Album",
                    Duration = FormatDuration(song.Duration),
                    SongData = song
                };
                Playlist_Songs_List.Items.Add(vm);
                idx++;
            }

            // 3) Cập nhật thông tin tổng số và thời lượng
            var totalSec = _currentPlaylistSongs.Sum(s => s.Duration);
            SelectedPlaylistInfoText.Text =
                $"{_currentPlaylistSongs.Count} songs • {FormatTotalDuration(TimeSpan.FromSeconds(totalSec))}";

            // 4) Tự động phát bài đầu tiên sau khi trộn
            PlaySongAtIndex(0);
        }

        private void BtnRemoveSongFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selectedVM = Playlist_Songs_List.SelectedItem as SongViewModel;
            if (selectedVM == null)
            {
                MessageBox.Show("Vui lòng chọn một bài hát để xóa.",
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa \"{selectedVM.Title}\" khỏi playlist?",
                "Confirm Remove",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Dùng đúng playlistId đã load
                _playlistRepo.RemoveSongFromPlaylist(
                    songId: selectedVM.SongData.SongId,
                    playlistId: _currentPlaylistId);

                // Reload lại playlist để cập nhật UI
                LoadPlaylist(_currentPlaylistId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa bài hát thất bại: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        // Sign out functionality
        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            // Clear current user information
            App.Current.Properties["CurrentUser"] = null;
            App.Current.Properties["UserRole"] = null;

            // Return to sign-in screen
            SignInScreen signInScreen = new SignInScreen(new UserService());
            signInScreen.Show();
            this.Close();
        }

        // Admin functionality
        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has admin role
            if (_currentUser != null && _currentUser.RoleId == 3)
            {
                // TODO: Open user management window
                MessageBox.Show("User management functionality will be implemented in a future update.",
                    "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("You don't have permission to access this feature.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has admin role
            if (_currentUser != null && _currentUser.RoleId == 3)
            {
                // TODO: Open content deletion window
                MessageBox.Show("Content deletion functionality will be implemented in a future update.",
                    "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("You don't have permission to access this feature.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Artist related functionality
        private void AddArtistButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has admin role or artist role
            if (_currentUser != null && (_currentUser.RoleId == 3 || _currentUser.RoleId == 2))
            {
                var addArtistWindow = new AddArtistWindow(_artistService);
                if (addArtistWindow.ShowDialog() == true)
                {
                    // Refresh the artists view
                    LoadArtists();
                }
            }
            else
            {
                MessageBox.Show("You don't have permission to add artists.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has admin role or artist role
            if (_currentUser != null && (_currentUser.RoleId == 3 || _currentUser.RoleId == 2))
            {
                var addAlbumWindow = new AddAlbumWindow(_albumService, _artistService);
                if (addAlbumWindow.ShowDialog() == true)
                {
                    // Refresh relevant views
                    LoadArtists();
                }
            }
            else
            {
                MessageBox.Show("You don't have permission to add albums.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // User functionality
        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("You need to sign in to create playlists.",
                    "Sign In Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var createWindow = new CreatePlayListWindow(_currentUser.UserId)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool? dialogResult = createWindow.ShowDialog();
            if (dialogResult == true)
            {
                // Tạo thành công, reload ngay
                LoadUserPlaylists();
            }
        }





        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("You need to sign in to add favorites.",
                    "Sign In Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Song currentSong = _songService.GetCurrentSong();
            if (currentSong != null)
            {
                try
                {
                    _songService.AddFavorite(_currentUser.UserId, currentSong.SongId);

                    MessageBox.Show($"'{currentSong.Title}' has been added to your favorites.",
                        "Added to Favorites", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadFavorites();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Adding to Favorites", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("No song is currently playing.",
                    "No Song Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void LoadHomeViewContent()
        {
            try
            {
                LoadTopSongs();
                LoadTopAlbums();
                LoadPopularArtists();
                LoadRecentlyPlayed();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading home view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load top songs based on play count
        private void LoadTopSongs()
        {
            try
            {
                TopSongsList.Items.Clear();

                // Get all songs and order by play count
                var allSongs = _songService.GetAll();
                var topSongs = allSongs
                    .OrderByDescending(s => s.PlayCount ?? 0)
                    .Take(10)
                    .ToList();

                foreach (var song in topSongs)
                {
                    // Create song card
                    Border songCard = CreateSongCard(song);
                    TopSongsList.Items.Add(songCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading top songs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a song card for the Top Songs list
        private Border CreateSongCard(Song song)
        {
            Border card = new Border
            {
                Width = 200,
                Height = 250,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525")),
                CornerRadius = new CornerRadius(6)
            };

            StackPanel content = new StackPanel();

            // Song thumbnail
            Border thumbnail = new Border
            {
                Width = 175,
                Height = 175,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                Margin = new Thickness(0, 12, 0, 8),
                CornerRadius = new CornerRadius(4)
            };

            TextBlock icon = new TextBlock
            {
                Text = "♫",
                FontSize = 70,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            thumbnail.Child = icon;
            content.Children.Add(thumbnail);

            // Song title
            TextBlock titleText = new TextBlock
            {
                Text = song.Title,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(12, 0, 12, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            content.Children.Add(titleText);

            // Artist name
            TextBlock artistText = new TextBlock
            {
                Text = song.Artist?.Name ?? "Unknown Artist",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3B3B3")),
                FontSize = 12,
                Margin = new Thickness(12, 4, 12, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            content.Children.Add(artistText);

            card.Child = content;

            // Add click event to play the song
            card.MouseLeftButtonUp += (s, e) => {
                _songService.PlaySong(song);
                UpdateNowPlayingInfo();
                PlayBtn.Content = "❚❚";
            };

            return card;
        }

        // Load top albums
        private void LoadTopAlbums()
        {
            try
            {
                TopAlbumsList.Items.Clear();

                // Get all albums
                var allAlbums = _albumService.GetAll();

                // Take up to 10 albums (or fewer if we don't have 10)
                var displayAlbums = allAlbums.Take(10).ToList();

                foreach (var album in displayAlbums)
                {
                    // Create album card
                    Border albumCard = CreateAlbumCard(album);
                    TopAlbumsList.Items.Add(albumCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading top albums: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create an album card
        private Border CreateAlbumCard(Album album)
        {
            Border card = new Border
            {
                Width = 200,
                Height = 250,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525")),
                CornerRadius = new CornerRadius(6)
            };

            StackPanel content = new StackPanel();

            // Album cover
            Border cover = new Border
            {
                Width = 175,
                Height = 175,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                Margin = new Thickness(0, 12, 0, 8),
                CornerRadius = new CornerRadius(4)
            };

            TextBlock icon = new TextBlock
            {
                Text = "💿",
                FontSize = 70,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            cover.Child = icon;
            content.Children.Add(cover);

            // Album title
            TextBlock titleText = new TextBlock
            {
                Text = album.Title,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(12, 0, 12, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            content.Children.Add(titleText);

            // Artist name
            TextBlock artistText = new TextBlock
            {
                Text = album.Artist?.Name ?? "Unknown Artist",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3B3B3")),
                FontSize = 12,
                Margin = new Thickness(12, 4, 12, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            content.Children.Add(artistText);

            card.Child = content;

            // Add click event to show album details
            card.MouseLeftButtonUp += (s, e) => {
                // TODO: Implement album detail view
                MessageBox.Show($"Album: {album.Title}", "Album Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            return card;
        }

        // Load popular artists
        private void LoadPopularArtists()
        {
            try
            {
                PopularArtistsList.Items.Clear();

                // Get all artists
                var allArtists = _artistService.GetAllArtists();

                // For now, just display the first 10 artists
                // In a real app, you would determine popularity based on metrics
                var popularArtists = allArtists.Take(10).ToList();

                foreach (var artist in popularArtists)
                {
                    // Create artist card
                    var artistCard = CreateArtistCardForHome(artist);
                    PopularArtistsList.Items.Add(artistCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading popular artists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create an artist card specifically for the home view
        private StackPanel CreateArtistCardForHome(Artist artist)
        {
            StackPanel card = new StackPanel
            {
                Width = 170,
                Height = 220
            };

            // Artist avatar
            Border avatar = new Border
            {
                Width = 170,
                Height = 170,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                CornerRadius = new CornerRadius(85)
            };

            // Initial letter
            TextBlock initial = new TextBlock
            {
                Text = !string.IsNullOrEmpty(artist.Name) ? artist.Name[0].ToString().ToUpper() : "A",
                FontSize = 80,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DB954")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            avatar.Child = initial;
            card.Children.Add(avatar);

            // Artist Name
            TextBlock nameText = new TextBlock
            {
                Text = artist.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            card.Children.Add(nameText);

            // Add click event to show artist details
            card.MouseLeftButtonUp += (s, e) => ShowArtistDetails(artist);

            return card;
        }

        // Load recently played songs
        private void LoadRecentlyPlayed()
        {
            try
            {
                RecentlyPlayedList.Items.Clear();

                // In a real app, you would get this from the listening history
                // For now, we'll just display some sample data
                var songs = _songService.GetAll();

                if (songs.Count > 0)
                {
                    int index = 1;
                    foreach (Song song in songs.Take(5))
                    {
                        // Create a view model for binding
                        var recentSong = new RecentlyPlayedViewModel
                        {
                            Title = song.Title,
                            Artist = song.Artist?.Name ?? "Unknown Artist",
                            Album = song.Album?.Title ?? "Unknown Album",
                            LastPlayed = "Today", // In a real app, get this from listening history
                            SongData = song
                        };

                        RecentlyPlayedList.Items.Add(recentSong);
                        index++;

                        // Only show 5 items for now
                        if (index > 5) break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recently played: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update the HomeButton_Click method to call LoadHomeViewContent
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(HomeView);
            HomeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
            LoadHomeViewContent();
        }

        // Modify the constructor to show HomeView on startup
        // This would go in your existing constructor after initialization
        private void InitializeHomeView()
        {
            // Show the HomeView by default instead of Favorites
            ShowView(HomeView);
            HomeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));

            // Load the home view content
            LoadHomeViewContent();
        }

        // ViewModel for recently played items
        public class RecentlyPlayedViewModel
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string LastPlayed { get; set; }
            public Song SongData { get; set; }
        }

        // ViewModel for song display in ListView
        public class SongViewModel
        {
            public string Index { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string Duration { get; set; }
            public string PlayCount { get; set; }
            public Song SongData { get; set; }
        }

        private void SongProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _userIsDraggingSlider = true;

            // Check if music was playing and store that state
            _wasPlayingBeforeDrag = _songService.IsPlaying();

            // Pause playback while dragging
            if (_wasPlayingBeforeDrag)
            {
                _songService.PausePlayback();
            }

            // Update the position immediately for single-click seeking
            Point clickPoint = e.GetPosition(SongProgressSlider);
            double sliderWidth = SongProgressSlider.ActualWidth;

            if (sliderWidth > 0)
            {
                double ratio = Math.Clamp(clickPoint.X / sliderWidth, 0, 1);
                SongProgressSlider.Value = ratio * 100;
            }
        }

        private void SongProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_userIsDraggingSlider) return;

            // Get the new position as a percentage of the song duration
            double newPositionPercent = SongProgressSlider.Value;

            // Convert to seconds
            double newPositionSeconds = (_songService.GetCurrentSongDuration() * newPositionPercent) / 100;

            // Set the position in the audio player
            _songService.SetPlaybackPosition(newPositionSeconds);

            // Resume playback if it was playing before
            if (_wasPlayingBeforeDrag)
            {
                _songService.ResumePlayback();
            }

            _userIsDraggingSlider = false;
        }

        private void SongProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Only respond to user input, not programmatic updates
            if (_userIsDraggingSlider)
            {
                // Update the current time display while dragging
                UpdateTimeDisplay(SongProgressSlider.Value);
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Lưu tên của view hiện tại để khôi phục sau này
            if (HomeView.Visibility == Visibility.Visible)
                App.Current.Properties["PreviousViewName"] = "HomeView";
            else if (MyMusicView.Visibility == Visibility.Visible)
                App.Current.Properties["PreviousViewName"] = "MyMusicView";
            else if (FavoritesView.Visibility == Visibility.Visible)
                App.Current.Properties["PreviousViewName"] = "FavoritesView";
            else if (PlaylistView.Visibility == Visibility.Visible)
                App.Current.Properties["PreviousViewName"] = "PlaylistView";
            else if (ArtistDetailView.Visibility == Visibility.Visible)
                App.Current.Properties["PreviousViewName"] = "ArtistDetailView";

            // Ẩn header (Grid.Row=0)
            foreach (UIElement element in ((Grid)this.Content).Children)
            {
                if (Grid.GetRow(element) == 0)
                    element.Visibility = Visibility.Collapsed;
            }

            // Lấy grid chính chứa nội dung (Grid.Row=1)
            var mainContentGrid = ((Grid)this.Content).Children.Cast<UIElement>()
                .FirstOrDefault(e => Grid.GetRow(e) == 1) as Grid;

            if (mainContentGrid != null)
            {
                // Ẩn thanh navigation bên trái (Grid.Column=0 trong mainContentGrid)
                foreach (UIElement element in mainContentGrid.Children)
                {
                    if (Grid.GetColumn(element) == 0)
                        element.Visibility = Visibility.Collapsed;
                }
            }

            // Ẩn thanh progress (Grid.Row=2) và controls phát nhạc (Grid.Row=3)
            foreach (UIElement element in ((Grid)this.Content).Children)
            {
                if (Grid.GetRow(element) == 2 || Grid.GetRow(element) == 3)
                    element.Visibility = Visibility.Collapsed;
            }

            // Ẩn tất cả các view
            HomeView.Visibility = Visibility.Collapsed;
            MyMusicView.Visibility = Visibility.Collapsed;
            FavoritesView.Visibility = Visibility.Collapsed;
            PlaylistView.Visibility = Visibility.Collapsed;
            ArtistDetailView.Visibility = Visibility.Collapsed;

            // Hiển thị form cập nhật hồ sơ
            UpdateProfilePanel.Visibility = Visibility.Visible;

            // Điền thông tin người dùng vào form
            if (_currentUser != null)
            {
                UpdateEmail.Text = _currentUser.Email ?? string.Empty;
                UpdateUsername.Text = _currentUser.Username ?? string.Empty;
                UpdateFullName.Text = _currentUser.FullName ?? string.Empty;
                UpdateProfilePicture.Text = _currentUser.ProfilePicture ?? string.Empty;
            }
            else
            {
                MessageBox.Show("Không có thông tin người dùng để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProfileSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentUser == null)
                {
                    MessageBox.Show("Không có thông tin người dùng để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra thông tin đầu vào
                string newEmail = UpdateEmail.Text.Trim();
                string newUsername = UpdateUsername.Text.Trim();
                string newFullName = UpdateFullName.Text.Trim();
                string newProfilePicture = UpdateProfilePicture.Text.Trim();

                if (string.IsNullOrWhiteSpace(newEmail) || string.IsNullOrWhiteSpace(newUsername))
                {
                    MessageBox.Show("Email và tên người dùng không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cập nhật thông tin người dùng
                _currentUser.Email = newEmail;
                _currentUser.Username = newUsername;
                _currentUser.FullName = newFullName;
                _currentUser.ProfilePicture = newProfilePicture;

                // Lưu vào database
                _userService.Update(_currentUser);

                // Cập nhật hiển thị
                UserNameText.Text = _currentUser.Username;

                // Thông báo thành công
                MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ẩn form cập nhật
                UpdateProfilePanel.Visibility = Visibility.Collapsed;

                // Nạp lại toàn bộ UI bằng cách tạo lại layout
                InvalidateVisual();

                // Hiển thị lại tất cả các thành phần chính
                foreach (UIElement element in ((Grid)this.Content).Children)
                {
                    // Hiển thị lại tất cả trừ UpdateProfilePanel
                    if (element != UpdateProfilePanel)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }

                // Lấy grid chính chứa nội dung (Grid.Row=1)
                var mainContentGrid = ((Grid)this.Content).Children.Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == 1) as Grid;

                if (mainContentGrid != null)
                {
                    foreach (UIElement element in mainContentGrid.Children)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }

                // Xác định view nào cần hiển thị lại
                string previousViewName = App.Current.Properties["PreviousViewName"] as string ?? "HomeView";

                switch (previousViewName)
                {
                    case "HomeView":
                        ShowView(HomeView);
                        HomeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
                        LoadHomeViewContent(); // Đảm bảo nội dung được nạp lại
                        break;
                    case "MyMusicView":
                        ShowView(MyMusicView);
                        MyMusicButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
                        if (ArtistsPanel.Children.Count == 0)
                            LoadArtists(); // Nạp lại nội dung artist nếu cần
                        break;
                    case "FavoritesView":
                        ShowView(FavoritesView);
                        FavoritesButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
                        LoadFavorites(); // Nạp lại danh sách yêu thích
                        break;
                    case "PlaylistView":
                        ShowView(PlaylistView);
                        break;
                    case "ArtistDetailView":
                        ShowView(ArtistDetailView);
                        break;
                    default:
                        ShowView(HomeView);
                        HomeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E3E"));
                        LoadHomeViewContent(); // Đảm bảo nội dung được nạp lại
                        break;
                }

                // Buộc cập nhật layout
                UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void UpdateTimeDisplay(double progressPercentage)
        {
            double totalDuration = _songService.GetCurrentSongDuration();
            double currentPosition = (totalDuration * progressPercentage) / 100;
            
            CurrentTimeText.Text = FormatTimeSpan(TimeSpan.FromSeconds(currentPosition));
            TotalTimeText.Text = FormatTimeSpan(TimeSpan.FromSeconds(totalDuration));
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            return time.Hours > 0 
                ? $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}" 
                : $"{time.Minutes}:{time.Seconds:D2}";
        }
        private void NowPlayingFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("You need to sign in to manage favorites.",
                    "Sign In Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Song currentSong = _songService.GetCurrentSong();
            if (currentSong == null)
            {
                MessageBox.Show("No song is currently playing.",
                    "No Song Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool isFavorite = _songService.IsFavorite(_currentUser.UserId, currentSong.SongId);
                if (isFavorite)
                {
                    _songService.RemoveFavorite(_currentUser.UserId, currentSong.SongId);
                    MessageBox.Show($"'{currentSong.Title}' has been removed from your favorites.",
                        "Removed from Favorites", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _songService.AddFavorite(_currentUser.UserId, currentSong.SongId);
                    MessageBox.Show($"'{currentSong.Title}' has been added to your favorites.",
                        "Added to Favorites", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateNowPlayingFavoriteButton();
                LoadFavorites(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing favorites: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}