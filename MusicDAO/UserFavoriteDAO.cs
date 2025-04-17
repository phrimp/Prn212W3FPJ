using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace MusicDAO
{
    public class UserFavoriteDAO
    {

        private MusicPlayerAppContext _dbContext;
        private static UserFavoriteDAO instance;

        public UserFavoriteDAO()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static UserFavoriteDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserFavoriteDAO();
                }
                return instance;
            }
        }

        public UserFavorite? GetOne(int id)
        {
            return _dbContext.UserFavorites
                .SingleOrDefault(a => a.UserId.Equals(id));
        }

        public List<UserFavorite> GetAll()
        {
            return _dbContext.UserFavorites
                .ToList();
        }

        public List<UserFavorite> GetAllById(int id)
        {
            return _dbContext.UserFavorites
                .Where(a=>a.UserId.Equals(id))
                .ToList();
        }

        public void Add(UserFavorite a)
        {
            UserFavorite? cur = GetOne(a.UserId);
            if (cur != null)
            {
                throw new Exception();
            }
            _dbContext.UserFavorites.Add(a);
            _dbContext.SaveChanges();
        }

        public void Update(UserFavorite a)
        {
            UserFavorite? cur = GetOne(a.UserId);
            if (cur == null)
            {
                throw new Exception();
            }
            _dbContext.Entry(cur).CurrentValues.SetValues(a);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            UserFavorite? cur = GetOne(id);
            if (cur != null)
            {
                _dbContext.UserFavorites.Remove(cur);
                _dbContext.SaveChanges();
            }
        }

    }
}
