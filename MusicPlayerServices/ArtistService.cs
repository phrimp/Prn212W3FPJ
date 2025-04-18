using MusicPlayerEntities;
using MusicPlayerRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public class ArtistService
    {
        private ArtistRepository _artistRepository;

        public ArtistService()
        {
            _artistRepository = ArtistRepository.Instance;
        }

        public Artist GetArtistById(int artistId)
            => _artistRepository.GetArtistById(artistId);

        public Artist GetArtistBySongId(int songId)
            => _artistRepository.GetArtistBySongId(songId);

    }
}
