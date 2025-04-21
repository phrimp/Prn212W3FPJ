using System;
using System.Collections.Generic;

namespace MusicPlayerEntities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string FullName { get; set; }

    public string ProfilePicture { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool? IsActive { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual Role Role { get; set; }
}