using MusicPlayerEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerRepositories
{
    public class ArtistRepository
    {

        private MusicPlayerAppContext _dbContext;
        private static ArtistRepository instance;
        private SongRepository songRepository;

        public ArtistRepository()
        {
            _dbContext = new MusicPlayerAppContext();
            songRepository = new SongRepository();
        }

        public static ArtistRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ArtistRepository();
                }
                return instance;
            }
        }

        public Artist GetArtistById(int artistId)
        {
            return _dbContext.Artists
                .FirstOrDefault(a => a.ArtistId == artistId);
        }

        public Artist GetArtistBySongId(int songId)
        {
            var song = songRepository.GetOne(songId);
            if (song == null)
            {
                throw new Exception("Song not found");
            }

            return _dbContext.Artists.FirstOrDefault(a => a.ArtistId == song.ArtistId);
        }
    }
}
