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
        public List<Song> GetMyFavotites(int id);
    }
}
