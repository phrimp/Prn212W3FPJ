using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;

namespace MusicPlayerRepositories
{
    public interface ISongService
    {
        public Song? GetOne(int id);

        public List<Song> GetAll();

        public void Add(Song a);

        public void Update(Song a);

        public void Delete(int id);

        public void PlaySong(Song song);
    }
}
