using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace MatchMusic.Models
{
    public class User
    {
        [Key]
        [Column(TypeName = "varchar(30)")]
        public string UserId { get; set; }
        
        [Required]
        [Column(TypeName = "nvarchar(60)")]
        public string UserName { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string UserCode { get; set; }
        public ICollection<UserMatch> MatchedUsers { get; set; } = new List<UserMatch>();

        public ICollection<UserArtist> UsersArtists { get; set; } = new List<UserArtist>();

        public ICollection<UserTrack> UsersTracks { get; set; } = new List<UserTrack>();

        [Column(TypeName = "varchar(100)")]
        public string UserProfilePicture { get; set; }
        
      
    }
}
