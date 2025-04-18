using System;
using System.Collections.Generic;

namespace MusicPlayerEntities;

public partial class Artist
{
    public int ArtistId { get; set; }

    public string Name { get; set; }

    public string Bio { get; set; }

    public string ImageUrl { get; set; }

    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
