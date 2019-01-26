using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserSide.Models
{
    public class Challenge
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public string Flag { get; set; }
        public string FileName { get; set; }

        public int CompetitionID { get; set; }
        public int CompetitionCategoryID { get; set; }

        public ICollection<TeamChallenge> TeamChallenges { get; set; }
    }
}
