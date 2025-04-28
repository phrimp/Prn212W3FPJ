using MusicPlayerEntities;
using MusicPlayerRepositories;
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
using System.Xml.Linq;

namespace MusicPlayerUI
{
    /// <summary>
    /// Interaction logic for CreatePlayListWindow.xaml
    /// </summary>
    public partial class CreatePlayListWindow : Window
    {
        private readonly int _userId;

        /// <summary>
        /// Initializes a new instance of CreatePlayListWindow.
        /// </summary>
        /// <param name="userId">ID of the user creating the playlist.</param>
        public CreatePlayListWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        /// <summary>
        /// Handles the Save button click: validates input and creates a new playlist.
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            var name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a playlist name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Build playlist entity
            var playlist = new Playlist
            {
                Name = name,
                //Description = txtDescription.Text.Trim(),
                //CoverImageUrl = txtCoverImageUrl.Text.Trim(),
                //IsPublic = chkIsPublic.IsChecked == true,
                UserId = _userId,
                CreatedDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            try
            {
                // Persist to database
                PlaylistRepository.Instance.CreateNewPlaylist(playlist);

                MessageBox.Show("Playlist created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Cancel button click: closes the window without saving.
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
