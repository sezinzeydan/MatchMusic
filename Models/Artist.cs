using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MatchMusic.Models
{
    public class Artist
    {
        [Key]
        [Column(TypeName = "varchar(30)")]
        public string ArtistId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(60)")]
        public string ArtistName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ArtistPicture { get; set; }

        public ICollection<ArtistMatch> MatchedArtists { get; set; } = new List<ArtistMatch>();

        public ICollection<UserArtist> UsersArtists { get; set; } = new List<UserArtist>();

    }
}
