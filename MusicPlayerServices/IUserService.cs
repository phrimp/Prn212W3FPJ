using MusicPlayerEntities;
using MusicPlayerRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public interface IUserService
    {
        public User GetOne(int id);

        public User? SignIn(string accountIdentify, string password);

        public List<User> GetAll();

        public void Add(User user);

        public void Update(User a);

        public void Delete(int id);

        public List<Song> GetMyFavotites(int id);
    }
}
