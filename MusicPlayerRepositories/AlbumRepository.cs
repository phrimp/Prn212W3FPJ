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
            return _dbContext.Albums.ToList();
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
    }
}
