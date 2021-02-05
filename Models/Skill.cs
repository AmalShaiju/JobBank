using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Skill
    {
        public Skill()
        {
            ApplicantSkills = new HashSet<ApplicantSkill>();
            PositionSkills = new HashSet<PositionSkill>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave Name of the skill blank.")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        public ICollection<ApplicantSkill> ApplicantSkills { get; set; }

        public ICollection<PositionSkill> PositionSkills { get; set; }

    }
}
