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

        public List<Artist> GetAllArtists()
            => _artistRepository.GetAllArtists();
        public void AddNewArtist(Artist artist) => _artistRepository.AddNewArtist(artist);
        public List<Artist> GetArtistsOrderByName() => _artistRepository.GetArtistOrderByName();

        public Artist GetArtistByUsername(string username)
        {
            // This implementation depends on how your data model connects users to artists
            // Here's a simple implementation assuming artists have a Username property
            return _artistRepository.GetAllArtists().FirstOrDefault(a => a.Name == username);
        }

        public List<Artist> GetAlbumsByArtistId(int artistId)
        {
            return _artistRepository.GetAllArtists().Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
