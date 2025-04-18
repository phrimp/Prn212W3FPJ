using System;
using System.Collections.Generic;

namespace MusicPlayerEntities;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
