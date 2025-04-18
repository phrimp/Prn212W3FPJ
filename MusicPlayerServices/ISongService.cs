using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;
using NAudio.Wave;

namespace MusicPlayerRepositories
{
    public interface ISongService
    {
        public Song? GetOne(int id);

        public List<Song> GetAll();

        public void Add(Song a);

        public void Update(Song a);

        public void Delete(int id);

        // Progress Tracking
        public float GetCurrentPosition();

        public float GetCurrentDuration();

        // Playback Control
        public void PlaySong(Song song);

        public void PlaySongById(int songId);

        public void PlayFromQueue();

        public void PausePlayback();

        public void ResumePlayback();

        public void TogglePlayPause();

        public void StopPlayback();

        public void NextSong();

        public void PreviousSong();

        public void SetVolume(float volume);

        public float GetVolume();

        public void ToggleLoop();

        public bool IsLooping();

        public bool IsPaused();

        public PlaybackState GetPlaybackState();

        // Queue Management
        public void LoadPlaylistToQueue(int playlistId, bool clearCurrentQueue = true);

        public void AddToQueue(Song song);

        public void AddToQueue(int songId);

        public Song RemoveFromQueue(int index);

        public void ClearQueue();

        public List<Song> GetCurrentQueue();

        public Song GetCurrentSong();

        public void Dispose();
    }
}