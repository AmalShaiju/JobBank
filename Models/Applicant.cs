using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Applicant : Auditable
    {
        public Applicant()
        {
            Applications = new HashSet<Application>();
            ApplicantSkills = new HashSet<ApplicantSkill>();
            ApplicantDocuments = new HashSet<ApplicantDocument>();
        }

        public int ID { get; set; }

        [Display(Name = "Applicant")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                    (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        [Display(Name = "Applicant")]
        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }//Added for concurrency

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(30, ErrorMessage = "Middle name cannot be more than 30 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave last name blank.")]
        [StringLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "You cannot leave the SIN number blank.")]
        [RegularExpression("^\\d{9}$", ErrorMessage = "The SIN number must be exactly 9 numeric digits.")]
        [StringLength(9)]//DS Note: we only include this to limit the size of the database field to 10
        public string SIN { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:(###) ###-####}", ApplyFormatInEditMode = false)]
        public Int64 Phone { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string eMail { get; set; }

        [Display(Name = "Retraining Program")]
        public int? RetrainingProgramID { get; set; }

        [Display(Name = "Retraining Program")]
        public RetrainingProgram RetrainingProgram { get; set; }
        public ApplicantPhoto ApplicantPhoto { get; set; }


        public ICollection<Application> Applications { get; set; }

        public ICollection<ApplicantDocument> ApplicantDocuments { get; set; }

        [Display(Name = "Skills")]
        public ICollection<ApplicantSkill> ApplicantSkills { get; set; }
    }
}
