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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicPlayerEntities;
using MusicPlayerRepositories;
using MusicPlayerServices;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for HomeScreen.xaml
    /// </summary>
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
        }

        private void LoadFavorites()
        {
            try
            {
                Favorite_List.Items.Clear();


                List<Song> favotites = _userService.GetMyFavotites(_currentUserId);
                if (favotites == null) { return; }

                int index = 1;
                foreach (Song s in favotites)
                {
                    Favorite_List.Items.Add(CreateSongListItem(s, index));
                    index++;
                }
            }
            catch (Exception ex) {
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
            SongRepository songRepository = new SongRepository();
            Song song = songRepository.GetOne(1);
            song_name.Text = song.Title;
            song_artist.Text = song.Artist?.Name ?? "Unknown Artist";
            PlayBtn.Content = "❚❚";
            songRepository.PlaySong(song);
        }
    }
}
