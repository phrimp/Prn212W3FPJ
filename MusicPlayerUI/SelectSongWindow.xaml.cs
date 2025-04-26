using MusicPlayerEntities;
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
using System.Windows.Shapes;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for SelectSongWindow.xaml
    /// </summary>
    public partial class SelectSongWindow : Window
    {
        public int SelectedSongId { get; private set; }

        public SelectSongWindow(List<Song> songs)
        {
            InitializeComponent();
            SongsListBox.ItemsSource = songs;
            if (songs.Count > 0)
                SongsListBox.SelectedIndex = 0;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (SongsListBox.SelectedValue is int id)
            {
                SelectedSongId = id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a song first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
