using MusicPlayerEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerRepositories
{
    public class PlaylistRepository
    {

        private MusicPlayerAppContext _dbContext;
        private static PlaylistRepository instance;

        public PlaylistRepository()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static PlaylistRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlaylistRepository();
                }
                return instance;
            }
        }

        public Playlist? GetOne(int id)
        {
            return _dbContext.Playlists.Where(p => p.PlaylistId == id).FirstOrDefault();
        }

        public List<Playlist> GetUserPlaylist(int userId)
        {
            var user_playlist = _dbContext.Playlists.Where(p => p.UserId == userId).ToList();
            return user_playlist;
        }

        public List<Song> GetSongsFromPlaylist(int playlistid)
        {
            var current_playlist = _dbContext.Playlists.Where(p => p.PlaylistId == playlistid).FirstOrDefault();
            if (current_playlist == null)
            {
                throw new Exception("Playlist not found");
            }
            var songs = _dbContext.PlaylistSongs.Where(p => p.PlaylistId == playlistid).Select(p => p.Song).ToList();
            return songs;
        }

        public void AddSongToPlaylist(int song_id, int playlist_id)
        {
            var create_date = DateTime.Now;

            var playlist = GetOne(playlist_id);
            if (playlist == null)
            {
                throw new Exception("Playlist not found");
            }

            var song = _dbContext.Songs.FirstOrDefault(s => s.SongId == song_id);
            if (song == null)
            {
                throw new Exception("Song not found");
            }

            var existingPlaylistSong = _dbContext.PlaylistSongs.FirstOrDefault(
                ps => ps.PlaylistId == playlist_id && ps.SongId == song_id);
            if (existingPlaylistSong != null)
            {
                throw new Exception("Song already exists in playlist");
            }

            int sortOrder = 1;
            var lastSong = _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlist_id)
                .OrderByDescending(ps => ps.SortOrder)
                .FirstOrDefault();

            if (lastSong != null)
            {
                sortOrder = lastSong.SortOrder + 1;
            }

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlist_id,
                SongId = song_id,
                AddedDate = create_date,
                SortOrder = sortOrder
            };

            _dbContext.PlaylistSongs.Add(playlistSong);

            playlist.LastUpdatedDate = create_date;

            _dbContext.SaveChanges();
        }

        public void DeletePlaylist(int playlistId)
        {
            var existingPlaylist = GetOne(playlistId);
            if (existingPlaylist == null)
            {
                throw new Exception("Playlist not found");
            }

            var playlistSongs = _dbContext.PlaylistSongs.Where(ps => ps.PlaylistId == playlistId).ToList();
            foreach (var playlistSong in playlistSongs)
            {
                _dbContext.PlaylistSongs.Remove(playlistSong);
            }

            _dbContext.Playlists.Remove(existingPlaylist);
            _dbContext.SaveChanges();
        }
        public void CreateNewPlaylist(Playlist playlist)
        {
            _dbContext.Add(playlist);
            _dbContext.SaveChanges();
        }

        public void UpdatePlaylist(Playlist updatedPlaylist)
        {
            var existingPlaylist = GetOne(updatedPlaylist.PlaylistId);
            if (existingPlaylist == null)
            {
                throw new Exception("Playlist not found");
            }

            existingPlaylist.Name = updatedPlaylist.Name;
            existingPlaylist.Description = updatedPlaylist.Description;
            existingPlaylist.IsPublic = updatedPlaylist.IsPublic;
            existingPlaylist.CoverImageUrl = updatedPlaylist.CoverImageUrl;
            existingPlaylist.LastUpdatedDate = DateTime.Now;

            _dbContext.SaveChanges();
        }
        
        public void RemoveSongFromPlaylist(int songId, int playlistId)
        {
            var existingPlaylist = GetOne(playlistId);
            if (existingPlaylist == null)
            {
                throw new Exception("Playlist not found");
            }

            var playlistSong = _dbContext.PlaylistSongs
                .FirstOrDefault(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
            {
                throw new Exception("Song not found in playlist");
            }

            _dbContext.PlaylistSongs.Remove(playlistSong);

            existingPlaylist.LastUpdatedDate = DateTime.Now;

            var remainingSongs = _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId && ps.SortOrder > playlistSong.SortOrder)
                .OrderBy(ps => ps.SortOrder)
                .ToList();

            foreach (var song in remainingSongs)
            {
                song.SortOrder--;
            }

            _dbContext.SaveChanges();
        }

        public List<Playlist> SearchPlaylists(string searchTerm, bool includePrivate = false, int? userId = null)
        {
            var query = _dbContext.Playlists.AsQueryable();

            if (!includePrivate)
            {
                query = query.Where(p => p.IsPublic == true);
            }
            else if (userId.HasValue)
            {
                query = query.Where(p => p.IsPublic == true || p.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm));
            }

            return query.ToList();
        }

        public int GetPlaylistSongCount(int playlistId)
        {
            return _dbContext.PlaylistSongs.Count(ps => ps.PlaylistId == playlistId);
        }

    }
}
