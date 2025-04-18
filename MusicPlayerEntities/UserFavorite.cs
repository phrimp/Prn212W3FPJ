using System;
using System.Collections.Generic;

namespace MusicPlayerEntities;

public partial class UserFavorite
{
    public int UserId { get; set; }

    public int SongId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual Song Song { get; set; }

    public virtual User User { get; set; }
}
