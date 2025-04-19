using MusicPlayerEntities;
using MusicPlayerRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public class GenreService
    {
        private GenreRepository genreRepository;
        public GenreService() { genreRepository = GenreRepository.Instance; }

        public List<Genre> GetAll() { return genreRepository.GetAll(); }
    }
}
