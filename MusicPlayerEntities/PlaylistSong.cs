using System;
using System.Collections.Generic;

namespace MusicPlayerEntities;

public partial class PlaylistSong
{
    public int PlaylistId { get; set; }

    public int SongId { get; set; }

    public DateTime? AddedDate { get; set; }

    public int SortOrder { get; set; }

    public virtual Playlist Playlist { get; set; }

    public virtual Song Song { get; set; }
}
