using Models;
using MusicDAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRepository
{
    public class SongRepository
    {
        public void PlaySong(Song song) => SongDAO.Instance.PlaySong(song);
        public Song GetASong(int id) => SongDAO.Instance.GetOne(id);
    }
}
