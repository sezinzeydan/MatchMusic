using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MatchMusic.Models
{
    public class UserMatch
    {
        [Key]
        [Column(TypeName = "varchar(30)")]
        public string  UserId { get; set; }
        public User User { get; set; }

        [Key]
        public int MatchId { get; set; }
        public Match Match { get; set; }

    }
}
