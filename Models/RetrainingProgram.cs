using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class RetrainingProgram
    {
        public RetrainingProgram()
        {
            Applicants = new HashSet<Applicant>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave Name of the retraining program blank.")]
        [StringLength(70, ErrorMessage = "Name cannot be more than 70 characters long.")]
        [DisplayFormat(NullDisplayText = "Not enrolled in any program.")]
        public string Name { get; set; }

        public ICollection<Applicant> Applicants { get; set; }
    }
}
