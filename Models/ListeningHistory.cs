using System;
using System.Collections.Generic;

namespace Models;

public partial class ListeningHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int SongId { get; set; }

    public DateTime? PlayedDate { get; set; }

    public int? PlayDuration { get; set; }

    public virtual Song Song { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
