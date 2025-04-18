using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;
using NAudio.Wave;

namespace MusicPlayerRepositories
{
    public class SongRepository
    {
        private MusicPlayerAppContext _dbContext;
        private static SongRepository instance;

        // Audio player fields
        private AudioFileReader currentAudioFile;
        private WaveOutEvent outputDevice;
        private Queue<Song> songQueue = new Queue<Song>();
        private Stack<Song> playHistory = new Stack<Song>();
        private Song currentSong;
        private bool isLooping = false;
        private float currentVolume = 1.0f;
        private bool isPaused = false;

        public SongRepository()
        {
            _dbContext = new MusicPlayerAppContext();
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        public static SongRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SongRepository();
                }
                return instance;
            }
        }

        public Song? GetOne(int id)
        {
            return _dbContext.Songs
                .SingleOrDefault(a => a.SongId.Equals(id));
        }

        public List<Song> GetAll()
        {
            var all_songs = _dbContext.Songs.ToList();

            return all_songs;
        }

        public void Add(Song a)
        {
            Song? cur = GetOne(a.SongId);
            if (cur != null)
            {
                throw new Exception();
            }
            _dbContext.Songs.Add(a);
            _dbContext.SaveChanges();
        }

        public void Update(Song a)
        {
            Song? cur = GetOne(a.SongId);
            if (cur == null)
            {
                throw new Exception();
            }
            _dbContext.Entry(cur).CurrentValues.SetValues(a);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            Song? cur = GetOne(id);
            if (cur != null)
            {
                _dbContext.Songs.Remove(cur);
                _dbContext.SaveChanges();
            }
        }

        #region Progress Tracking

        public float GetCurrentPosition()
        {
            if (currentAudioFile != null)
            {
                return (float)currentAudioFile.CurrentTime.TotalSeconds;
            }
            return 0;
        }

        public float GetCurrentDuration()
        {
            if (currentAudioFile != null)
            {
                return (float)currentAudioFile.TotalTime.TotalSeconds;
            }
            return 0;
        }

        #endregion

        #region Queue Management

        public void LoadPlaylistToQueue(int playlistId, bool clearCurrentQueue = true)
        {
            var playlistRepository = new PlaylistRepository();
            var songs = playlistRepository.GetSongsFromPlaylist(playlistId);

            if (clearCurrentQueue)
            {
                ClearQueue();
            }

            foreach (var song in songs)
            {
                songQueue.Enqueue(song);
            }
        }

        public void AddToQueue(Song song)
        {
            songQueue.Enqueue(song);
        }

        public void AddToQueue(int songId)
        {
            var song = GetOne(songId);
            if (song != null)
            {
                songQueue.Enqueue(song);
            }
        }

        public Song RemoveFromQueue(int index)
        {
            if (index < 0 || index >= songQueue.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }

            var tempQueue = new Queue<Song>();
            Song removedSong = null;

            for (int i = 0; i < songQueue.Count; i++)
            {
                var song = songQueue.Dequeue();
                if (i != index)
                {
                    tempQueue.Enqueue(song);
                }
                else
                {
                    removedSong = song;
                }
            }

            songQueue = tempQueue;
            return removedSong;
        }

        public void ClearQueue()
        {
            songQueue.Clear();
            playHistory.Clear();
        }

        public List<Song> GetCurrentQueue()
        {
            return songQueue.ToList();
        }

        public Song GetCurrentSong()
        {
            return currentSong;
        }

        #endregion

        #region Playback Control

        public void PlaySong(Song song)
        {
            StopPlayback();

            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string songPath = Path.Combine(baseDirectory, "..", "..", "..", "Assets", "Songs", song.FilePath);

                currentAudioFile = new AudioFileReader(songPath);
                outputDevice.Init(currentAudioFile);
                currentAudioFile.Volume = currentVolume;

                currentSong = song;
                if (currentSong != null)
                {
                    // Update play count
                    currentSong.PlayCount = (currentSong.PlayCount ?? 0) + 1;
                    Update(currentSong);

                    // RecordListeningHistory(currentSong);
                }

                outputDevice.Play();
                isPaused = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error playing song: {ex.Message}");
            }
        }

        public void PlaySongById(int songId)
        {
            var song = GetOne(songId);
            if (song != null)
            {
                PlaySong(song);
            }
        }

        public void PlayFromQueue()
        {
            if (songQueue.Count > 0)
            {
                var nextSong = songQueue.Dequeue();
                if (currentSong != null)
                {
                    playHistory.Push(currentSong);
                }
                PlaySong(nextSong);
            }
        }

        public void PausePlayback()
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                isPaused = true;
            }
        }

        public void ResumePlayback()
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                isPaused = false;
            }
        }

        public void TogglePlayPause()
        {
            if (isPaused)
            {
                ResumePlayback();
            }
            else
            {
                PausePlayback();
            }
        }

        public void StopPlayback()
        {
            if (outputDevice != null)
            {
                if (outputDevice.PlaybackState == PlaybackState.Playing ||
                    outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    outputDevice.Stop();
                }
            }

            if (currentAudioFile != null)
            {
                currentAudioFile.Dispose();
                currentAudioFile = null;
            }
        }

        public void NextSong()
        {
            if (songQueue.Count > 0)
            {
                if (currentSong != null)
                {
                    playHistory.Push(currentSong);
                }
                PlayFromQueue();
            }
            else if (isLooping && currentSong != null)
            {
                PlaySong(currentSong); // Replay the current song if looping is enabled
            }
        }

        public void PreviousSong()
        {
            if (playHistory.Count > 0)
            {
                // If there's a current song, add it back to the front of the queue
                if (currentSong != null)
                {
                    var tempQueue = new Queue<Song>();
                    tempQueue.Enqueue(currentSong);
                    foreach (var song in songQueue)
                    {
                        tempQueue.Enqueue(song);
                    }
                    songQueue = tempQueue;
                }

                var previousSong = playHistory.Pop();
                PlaySong(previousSong);
            }
        }

        public void SetVolume(float volume)
        {
            // Ensure volume is between 0 and 1
            volume = Math.Clamp(volume, 0.0f, 1.0f);
            currentVolume = volume;

            if (currentAudioFile != null)
            {
                currentAudioFile.Volume = volume;
            }
        }

        public float GetVolume()
        {
            return currentVolume;
        }

        public void ToggleLoop()
        {
            isLooping = !isLooping;
        }

        public bool IsLooping()
        {
            return isLooping;
        }

        public bool IsPaused()
        {
            return isPaused;
        }

        public PlaybackState GetPlaybackState()
        {
            if (outputDevice != null)
            {
                return outputDevice.PlaybackState;
            }
            return PlaybackState.Stopped;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // When current song playback has completed
            if (e.Exception == null)
            {
                if (songQueue.Count > 0)
                {
                    // Play next song automatically
                    NextSong();
                }
                else if (isLooping && currentSong != null)
                {
                    // Play the same song again if looping is enabled
                    PlaySong(currentSong);
                }
            }
        }

        #endregion

        // Cleanup resources when done
        public void Dispose()
        {
            StopPlayback();

            if (outputDevice != null)
            {
                outputDevice.Dispose();
                outputDevice = null;
            }
        }

        #region Artist Song Management
        // Add these methods to your SongRepository.cs

        public List<Song> GetSongsByArtist(int artistId)
        {
            return _dbContext.Songs
                .Where(s => s.ArtistId == artistId)
                .OrderBy(s => s.Title)
                .ToList();
        }

        public List<Song> GetSongsByArtistName(string artistName)
        {
            return _dbContext.Songs
                .Where(s => s.Artist.Name.Contains(artistName))
                .OrderBy(s => s.Title)
                .ToList();
        }

        public void UpdateSongArtist(int songId, int newArtistId)
        {
            var song = GetOne(songId);
            if (song == null)
            {
                throw new Exception("Song not found");
            }

            var artist = _dbContext.Artists.FirstOrDefault(a => a.ArtistId == newArtistId);
            if (artist == null)
            {
                throw new Exception("Artist not found");
            }

            song.ArtistId = newArtistId;
            _dbContext.SaveChanges();
        }

        public List<Artist> GetAllArtists()
        {
            return _dbContext.Artists
                .OrderBy(a => a.Name)
                .ToList();
        }

        public List<Song> SearchSongsByArtist(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Song>();
            }

            searchTerm = searchTerm.ToLower();

            return _dbContext.Songs
                .Where(s => s.Artist.Name.ToLower().Contains(searchTerm))
                .OrderBy(s => s.Artist.Name)
                .ThenBy(s => s.Title)
                .ToList();
        }

        public List<Song> GetTopSongsByArtist(int artistId, int count = 5)
        {
            return _dbContext.Songs
                .Where(s => s.ArtistId == artistId)
                .OrderByDescending(s => s.PlayCount)
                .Take(count)
                .ToList();
        }

        public Dictionary<int, int> GetSongCountByArtist()
        {
            var result = new Dictionary<int, int>();

            var query = _dbContext.Artists
                .Select(a => new
                {
                    ArtistId = a.ArtistId,
                    SongCount = a.Songs.Count
                })
                .ToList();

            foreach (var item in query)
            {
                result.Add(item.ArtistId, item.SongCount);
            }

            return result;
        }
        #endregion
    }
}