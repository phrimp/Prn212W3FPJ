using Microsoft.EntityFrameworkCore;
using MusicPlayerEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerRepositories
{
    public class AlbumRepository
    {

        private MusicPlayerAppContext _dbContext;
        private static AlbumRepository instance;

        public AlbumRepository()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static AlbumRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AlbumRepository();
                }
                return instance;
            }
        }

        public List<Album> GetAll() {
            return _dbContext.Albums.Include(a => a.Artist).ToList();
        }

        public void Add(Album album) {
            try
            {
                _dbContext.Add(album);  
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public List<Album> GetByArtistId(int artistId)
        {
            try
            {
                return _dbContext.Albums
                    .Where(album => album.ArtistId == artistId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving albums for artist: {ex.Message}");
            }
        }
    }
}
