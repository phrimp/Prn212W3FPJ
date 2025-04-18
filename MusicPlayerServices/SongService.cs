using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;

namespace MusicPlayerRepositories
{
    public class SongService
    {
        public Song? GetOne(int id)
        => SongRepository.Instance.GetOne(id);

        public List<Song> GetAll()
        => SongRepository.Instance.GetAll();

        public void Add(Song a)
        => SongRepository.Instance.Add(a);

        public void Update(Song a)
        => SongRepository.Instance.Update(a);

        public void Delete(int id)
        => SongRepository.Instance.Delete(id);

        public void PlaySong(Song song)
        => SongRepository.Instance.PlaySong(song);
    }
}
