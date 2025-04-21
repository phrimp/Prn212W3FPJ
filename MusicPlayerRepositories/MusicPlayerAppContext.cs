using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MusicPlayerEntities;

public partial class MusicPlayerAppContext : DbContext
{
    public MusicPlayerAppContext()
    {
    }

    public MusicPlayerAppContext(DbContextOptions<MusicPlayerAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Album> Albums { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<ListeningHistory> ListeningHistories { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistSong> PlaylistSongs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(GetConnectionString());

    #region GetConnectionString()
    String GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        return config.GetConnectionString("DBConnString");
    }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.AlbumId).HasName("PK__Albums__97B4BE17DC4E9181");

            entity.Property(e => e.AlbumId).HasColumnName("AlbumID");
            entity.Property(e => e.ArtistId).HasColumnName("ArtistID");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(255)
                .HasColumnName("CoverImageURL");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Artist).WithMany(p => p.Albums)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Albums_Artists");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.ArtistId).HasName("PK__Artists__25706B703AF60FF4");

            entity.Property(e => e.ArtistId).HasColumnName("ArtistID");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PK__Genres__0385055EDAD90CE5");

            entity.HasIndex(e => e.Name, "UQ__Genres__737584F658DA1555").IsUnique();

            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<ListeningHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Listenin__4D7B4ADD9FA18E3F");

            entity.ToTable("ListeningHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.PlayedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Song).WithMany(p => p.ListeningHistories)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ListeningHistory_Songs");

            entity.HasOne(d => d.User).WithMany(p => p.ListeningHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ListeningHistory_Users");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId).HasName("PK__Playlist__B30167805B0273A2");

            entity.Property(e => e.PlaylistId).HasColumnName("PlaylistID");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(255)
                .HasColumnName("CoverImageURL");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.LastUpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Playlists_Users");
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.SongId }).HasName("PK__Playlist__D22F5AEFFF390754");

            entity.Property(e => e.PlaylistId).HasColumnName("PlaylistID");
            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.PlaylistId)
                .HasConstraintName("FK_PlaylistSongs_Playlists");

            entity.HasOne(d => d.Song).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlaylistSongs_Songs");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A7DBA9E07");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.SongId).HasName("PK__Songs__12E3D6F73E940B55");

            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.AlbumId).HasColumnName("AlbumID");
            entity.Property(e => e.ArtistId).HasColumnName("ArtistID");
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.PlayCount).HasDefaultValue(0);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Album).WithMany(p => p.Songs)
                .HasForeignKey(d => d.AlbumId)
                .HasConstraintName("FK_Songs_Albums");

            entity.HasOne(d => d.Artist).WithMany(p => p.Songs)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Songs_Artists");

            entity.HasOne(d => d.Genre).WithMany(p => p.Songs)
                .HasForeignKey(d => d.GenreId)
                .HasConstraintName("FK_Songs_Genres");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC87E3DF80");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E415E00708").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B01B3AF3").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ProfilePicture).HasMaxLength(255);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.RoleId)
                .HasColumnName("RoleID")
                .HasDefaultValue(1);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SongId }).HasName("PK__UserFavo__76A6F1C32D06F55D");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Song).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.SongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFavorites_Songs");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserFavorites_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}