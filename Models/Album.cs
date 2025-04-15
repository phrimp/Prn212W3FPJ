using System;
using System.Collections.Generic;

namespace Models;

public partial class Album
{
    public int AlbumId { get; set; }

    public string Title { get; set; } = null!;

    public int ArtistId { get; set; }

    public int? ReleaseYear { get; set; }

    public string? CoverImageUrl { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
