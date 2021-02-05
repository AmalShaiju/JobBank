using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Application : Auditable
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Comments blank.")]
        [DataType(DataType.MultilineText)]
        [MinLength(20, ErrorMessage = "The description must be at least 20 characters long.")]
        [MaxLength(2000, ErrorMessage = "The description cannot be more than 2000 characters long")]
        public string Comments { get; set; }

        [Display(Name = "Posting")]
        [Required(ErrorMessage = "You must select the Posting the applicaiton is for.")]
        public int PostingID { get; set; }

        public Posting Posting { get; set; }

        [Display(Name = "Applicant")]
        [Required(ErrorMessage = "You must select the Applicant who is applying for the job.")]
        public int ApplicantID { get; set; }

        public Applicant Applicant { get; set; }

    }
}
