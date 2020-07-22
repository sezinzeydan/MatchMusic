using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MatchMusic.Models
{
    public class Match
    {
        [Key]
        public int MatchId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime MatchDate { get; set; }

        public int MatchRate { get; set; }

        [Required]
        [Column(TypeName =  "nvarchar(125)")]
        public string MatchName { get; set; }

        public ICollection<TrackMatch> MatchedTracks { get; set; } = new List<TrackMatch>();
        public ICollection<ArtistMatch> MatchedArtists { get; set; } = new List<ArtistMatch>();

        public ICollection<UserMatch> MatchedUsers  { get; set; } = new List<UserMatch>();

    }
}
