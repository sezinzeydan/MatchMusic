using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MatchMusic.Models
{
    public class UserArtist
    {
        [Key]
        [Column(TypeName = "varchar(30)")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Key]
        [Column(TypeName = "varchar(30)")]
        public string ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
