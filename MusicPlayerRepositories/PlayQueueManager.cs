using System;
using System.Collections.Generic;
using System.Linq;
using MusicPlayerEntities;

namespace MusicPlayerRepositories
{
    public class PlayQueueManager
    {
        private static PlayQueueManager instance;

        private List<Song> _queuedSongs = new List<Song>();
        private int _currentIndex = -1;
        private bool _isLooping = false;
        private bool _isShuffled = false;

        private List<Song> _originalOrder = new List<Song>();

        // Events
        public event EventHandler<Song> CurrentSongChanged;
        public event EventHandler QueueChanged;

        private PlayQueueManager() { }

        public static PlayQueueManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayQueueManager();
                }
                return instance;
            }
        }

        public Song GetCurrentSong()
        {
            if (_queuedSongs.Count == 0 || _currentIndex < 0 || _currentIndex >= _queuedSongs.Count)
            {
                return null;
            }

            return _queuedSongs[_currentIndex];
        }

        public List<Song> GetQueuedSongs()
        {
            return new List<Song>(_queuedSongs);
        }

        public int GetCurrentIndex()
        {
            return _currentIndex;
        }

        public void LoadFromPlaylist(int playlistId, bool clearExisting = true)
        {
            try
            {
                var playlistRepository = PlaylistRepository.Instance;
                var songs = playlistRepository.GetSongsFromPlaylist(playlistId);

                SetQueue(songs, clearExisting);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading playlist: {ex.Message}");
                throw;
            }
        }

        public void SetQueue(List<Song> songs, bool clearExisting = true)
        {
            if (clearExisting)
            {
                _queuedSongs.Clear();
                _originalOrder.Clear();
                _currentIndex = -1;
            }

            if (songs != null && songs.Count > 0)
            {
                _queuedSongs.AddRange(songs);
                _originalOrder.AddRange(songs);

                if (_currentIndex < 0)
                {
                    _currentIndex = 0;
                    CurrentSongChanged?.Invoke(this, GetCurrentSong());
                }
            }

            QueueChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddToQueue(Song song)
        {
            if (song != null)
            {
                _queuedSongs.Add(song);
                _originalOrder.Add(song);

                if (_currentIndex < 0)
                {
                    _currentIndex = 0;
                    CurrentSongChanged?.Invoke(this, GetCurrentSong());
                }

                QueueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AddToQueue(List<Song> songs)
        {
            if (songs != null && songs.Count > 0)
            {
                _queuedSongs.AddRange(songs);
                _originalOrder.AddRange(songs);

                if (_currentIndex < 0)
                {
                    _currentIndex = 0;
                    CurrentSongChanged?.Invoke(this, GetCurrentSong());
                }

                QueueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveFromQueue(int queueIndex)
        {
            if (queueIndex >= 0 && queueIndex < _queuedSongs.Count)
            {
                var songToRemove = _queuedSongs[queueIndex];
                _queuedSongs.RemoveAt(queueIndex);

                _originalOrder.Remove(songToRemove);

                // Adjust current index if necessary
                if (queueIndex < _currentIndex)
                {
                    _currentIndex--;
                }
                else if (queueIndex == _currentIndex)
                {
                    CurrentSongChanged?.Invoke(this, GetCurrentSong());
                }

                QueueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearQueue()
        {
            _queuedSongs.Clear();
            _originalOrder.Clear();
            _currentIndex = -1;

            CurrentSongChanged?.Invoke(this, null);
            QueueChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool MoveToNext()
        {
            if (_queuedSongs.Count == 0)
            {
                return false;
            }

            if (_currentIndex < _queuedSongs.Count - 1)
            {
                _currentIndex++;
                CurrentSongChanged?.Invoke(this, GetCurrentSong());
                return true;
            }
            else if (_isLooping)
            {
                _currentIndex = 0;
                CurrentSongChanged?.Invoke(this, GetCurrentSong());
                return true;
            }

            return false;
        }

        public bool MoveToPrevious()
        {
            if (_queuedSongs.Count == 0)
            {
                return false;
            }

            if (_currentIndex > 0)
            {
                _currentIndex--;
                CurrentSongChanged?.Invoke(this, GetCurrentSong());
                return true;
            }
            else if (_isLooping)
            {
                _currentIndex = _queuedSongs.Count - 1;
                CurrentSongChanged?.Invoke(this, GetCurrentSong());
                return true;
            }

            return false;
        }

        public bool MoveToIndex(int index)
        {
            if (index >= 0 && index < _queuedSongs.Count)
            {
                _currentIndex = index;
                CurrentSongChanged?.Invoke(this, GetCurrentSong());
                return true;
            }

            return false;
        }

        public bool ToggleLoopMode()
        {
            _isLooping = !_isLooping;
            return _isLooping;
        }

        public bool IsLooping()
        {
            return _isLooping;
        }

        public bool ToggleShuffle()
        {
            if (!_isShuffled)
            {
                // Save original order if not already shuffled
                _originalOrder = new List<Song>(_queuedSongs);

                // Fisher-Yates shuffle algorithm
                Random rng = new Random();

                // Preserve current song
                Song currentSong = GetCurrentSong();

                // Remove current song from shuffle
                var songsToShuffle = _queuedSongs.Where((s, i) => i != _currentIndex).ToList();

                // Shuffle remaining songs
                int n = songsToShuffle.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    Song value = songsToShuffle[k];
                    songsToShuffle[k] = songsToShuffle[n];
                    songsToShuffle[n] = value;
                }

                // Rebuild queue with current song at current index
                _queuedSongs.Clear();
                if (_currentIndex > 0)
                {
                    _queuedSongs.AddRange(songsToShuffle.Take(_currentIndex));
                }
                _queuedSongs.Add(currentSong);
                if (_currentIndex < songsToShuffle.Count)
                {
                    _queuedSongs.AddRange(songsToShuffle.Skip(_currentIndex));
                }

                _isShuffled = true;
            }
            else
            {
                // Restore original order
                Song currentSong = GetCurrentSong();
                _queuedSongs = new List<Song>(_originalOrder);

                // Find the new index of the current song
                _currentIndex = _queuedSongs.IndexOf(currentSong);
                if (_currentIndex < 0 && _queuedSongs.Count > 0)
                {
                    _currentIndex = 0;
                }

                _isShuffled = false;
            }

            QueueChanged?.Invoke(this, EventArgs.Empty);
            return _isShuffled;
        }

        public bool IsShuffled()
        {
            return _isShuffled;
        }

        public void ReorderQueue(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < _queuedSongs.Count &&
                newIndex >= 0 && newIndex < _queuedSongs.Count &&
                oldIndex != newIndex)
            {
                // Remember the current song
                Song currentSong = GetCurrentSong();

                // Move the song
                Song song = _queuedSongs[oldIndex];
                _queuedSongs.RemoveAt(oldIndex);
                _queuedSongs.Insert(newIndex, song);

                // Update current index to keep the same song playing
                _currentIndex = _queuedSongs.IndexOf(currentSong);

                QueueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}