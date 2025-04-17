using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace MusicDAO
{
    public class UserDAO
    {

        private MusicPlayerAppContext _dbContext;
        private static UserDAO instance;

        public UserDAO()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static UserDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserDAO();
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

    }
}