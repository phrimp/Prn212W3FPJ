using System;
using System.Collections.Generic;

namespace Models;

public partial class Song
{
    public int SongId { get; set; }

    public string Title { get; set; } = null!;

    public int? AlbumId { get; set; }

    public int ArtistId { get; set; }

    public int? GenreId { get; set; }

    public int Duration { get; set; }

    public string FilePath { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public int? PlayCount { get; set; }

    public virtual Album? Album { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual Genre? Genre { get; set; }

    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
}
