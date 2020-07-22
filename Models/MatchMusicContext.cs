using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MatchMusic.Models
{
    public class MatchMusicContext : DbContext
    {
        public MatchMusicContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<ArtistMatch> MatchedArtists { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<TrackMatch> MatchedTracks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserArtist> UsersArtists { get; set; }
        public DbSet<UserMatch> MatchedUsers { get; set; }
        public DbSet<UserTrack> UsersTracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArtistMatch>()
                .HasKey(am => new {am.ArtistId, am.MatchId});
            modelBuilder.Entity<ArtistMatch>()
                .HasOne(am => am.Artist)
                .WithMany(a => a.MatchedArtists)
                .HasForeignKey(am => am.ArtistId);
            modelBuilder.Entity<ArtistMatch>()
                .HasOne(am => am.Match)
                .WithMany(a => a.MatchedArtists)
                .HasForeignKey(am => am.MatchId);


            modelBuilder.Entity<TrackMatch>()
                .HasKey(tm => new {tm.TrackId, tm.MatchId});
            modelBuilder.Entity<TrackMatch>()
                .HasOne(tm => tm.Track)
                .WithMany(t => t.MatchedTracks)
                .HasForeignKey(tm => tm.TrackId);
            modelBuilder.Entity<TrackMatch>()
                .HasOne(tm => tm.Match)
                .WithMany(m => m.MatchedTracks)
                .HasForeignKey(tm => tm.MatchId);


            modelBuilder.Entity<UserArtist>()
                .HasKey(ua => new {ua.UserId, ua.ArtistId});
            modelBuilder.Entity<UserArtist>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UsersArtists)
                .HasForeignKey(ua => ua.UserId);
            modelBuilder.Entity<UserArtist>()
                .HasOne(ua => ua.Artist)
                .WithMany(u => u.UsersArtists)
                .HasForeignKey(ua => ua.ArtistId);


            modelBuilder.Entity<UserMatch>()
                .HasKey(um => new {um.UserId, um.MatchId});
            modelBuilder.Entity<UserMatch>()
                .HasOne(um => um.User)
                .WithMany(u => u.MatchedUsers)
                .HasForeignKey(um => um.UserId);
            modelBuilder.Entity<UserMatch>()
                .HasOne(um => um.Match)
                .WithMany(m => m.MatchedUsers)
                .HasForeignKey(um => um.MatchId);

            modelBuilder.Entity<UserTrack>()
                .HasKey(ut => new {ut.UserId, ut.TrackId});
            modelBuilder.Entity<UserTrack>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UsersTracks)
                .HasForeignKey(ut => ut.UserId);
            modelBuilder.Entity<UserTrack>()
                .HasOne(ut => ut.Track)
                .WithMany(t => t.UsersTracks)
                .HasForeignKey(ut => ut.TrackId);

        }
    }
}
