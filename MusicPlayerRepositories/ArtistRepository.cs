using Microsoft.EntityFrameworkCore;
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

        public List<Artist> GetAllArtists()
        {
            try
            {
                return _dbContext.Artists
            .AsNoTracking()  // Add this line
            .OrderBy(a => a.Name)
            .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllArtists: {ex.Message}");
                return new List<Artist>();
            }
        }

        public void AddNewArtist(Artist artist)
        {
            try
            {
                _dbContext.Artists.Add(artist);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
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

        public List<Artist> GetArtistOrderByName()
        {
            var artists = _dbContext.Artists.OrderBy(a => a.Name).ToList();
            return artists;
        }
    }
}
