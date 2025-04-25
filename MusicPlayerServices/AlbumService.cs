using MusicPlayerEntities;
using MusicPlayerRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public class AlbumService
    {
        private AlbumRepository albumRepository;
        public AlbumService() { albumRepository = AlbumRepository.Instance; }

        public List<Album> GetAll() => albumRepository.GetAll();
        public void AddNewAlbum(Album album) => albumRepository.Add(album);

        public List<Album> GetByArtistId(int id) => albumRepository.GetByArtistId(id);
    }
}
