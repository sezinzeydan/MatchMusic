using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MatchMusic.Models
{
    public class Track
    {
        [Key]
        [Column(TypeName = "varchar(30)")]
        public string TrackId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(60)")]
        public string TrackName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string TrackAlbumPicture { get; set; }
        public ICollection<TrackMatch> MatchedTracks  { get; set; } = new List<TrackMatch>();

        public ICollection<UserTrack> UsersTracks { get; set; } = new List<UserTrack>();
    }
}
