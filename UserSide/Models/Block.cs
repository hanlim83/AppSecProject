using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Block
    {
        [Key]
        public int ID { get; set; }
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy} | {0:hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime TimeStamp { get; set; }
        [ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }
        [ForeignKey("TeamID")]
        public int TeamID { get; set; }
        [ForeignKey("ChallengeID")]
        public int ChallengeID { get; set; }
        [ForeignKey("TeamChallengeID")]
        public int TeamChallengeID { get; set; }
        public int Score { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
    }
}
