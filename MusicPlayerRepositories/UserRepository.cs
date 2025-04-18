using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;

namespace MusicPlayerRepositories
{
    public class UserRepository
    {

        private MusicPlayerAppContext _dbContext;
        private static UserRepository instance;

        public UserRepository()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static UserRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserRepository();
                }
                return instance;
            }
        }

        public User? GetOne(int id)
        {
            return _dbContext.Users
                .SingleOrDefault(a => a.UserId.Equals(id));
        }

        public List<User> GetAll()
        {
            return _dbContext.Users
                .ToList();
        }

        public void Add(User a)
        {
            User cur = GetOne(a.UserId);
            if (cur != null)
            {
                throw new Exception();
            }
            _dbContext.Users.Add(a);
            _dbContext.SaveChanges();
        }

        public void Update(User a)
        {
            User? cur = GetOne(a.UserId);
            if (cur == null)
            {
                throw new Exception();
            }
            _dbContext.Entry(cur).CurrentValues.SetValues(a);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            User? cur = GetOne(id);
            if (cur != null)
            {
                _dbContext.Users.Remove(cur);
                _dbContext.SaveChanges();
            }
        }

        public List<Song> GetMyFavotites(int id) {
            User? cur = GetOne(id);
            if (cur == null)
            {
                throw new Exception("User not found");
            }

            var favoriteSongs = _dbContext.UserFavorites.Where(uf => uf.UserId == id).Select(uf => uf.Song).ToList();
            return favoriteSongs;
        }
    }
}
