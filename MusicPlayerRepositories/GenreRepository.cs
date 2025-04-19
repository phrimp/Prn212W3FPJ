using MusicPlayerEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerRepositories
{
    public class GenreRepository
    {
        private MusicPlayerAppContext _dbContext;
        private static GenreRepository instance;

        public GenreRepository()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static GenreRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GenreRepository();
                }
                return instance;
            }
        }

        public List<Genre> GetAll()
        {
            return _dbContext.Genres.ToList();
        }
    }
}
