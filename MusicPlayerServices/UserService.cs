using MusicPlayerEntities;
using MusicPlayerRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public class UserService
    {
        public User GetOne(int id) => UserRepository.Instance.GetOne(id);
        public List<Song> GetMyFavotites(int id) => UserRepository.Instance.GetMyFavotites(id);
    }
}
