using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Position : Auditable
    {
        public Position()
        {
            Postings = new HashSet<Posting>();
            PositionSkills = new HashSet<PositionSkill>();
        }

        public int ID { get; set; }

        [Display(Name = "Position Name")]
        [Required(ErrorMessage = "You cannot leave the Name blank.")]
        [StringLength(255, ErrorMessage = "Name cannot be more than 255 characters long.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You cannot leave the description blank.")]
        [MinLength(20,ErrorMessage = "The description must be at least 20 characters long.")]
        [MaxLength(511, ErrorMessage = "The description cannot be more than 511 characters long")]
        //[StringLength(511, MinimumLength =20, ErrorMessage = "The description must be from 20 to 511 characters long")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "You must enter the salary amount.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(9,2)")]//So only 5 bytes to store in SQL Server
        [Range(0, 9999999.99, ErrorMessage = "Invalid price.")]
        public decimal Salary { get; set; }

        [Display(Name = "Occupation")]
        [Required(ErrorMessage = "You must select the Occupation.")]
        public int OccupationID { get; set; }

        public Occupation Occupation { get; set; }

        public ICollection<Posting> Postings { get; set; }

        public ICollection<PositionSkill> PositionSkills { get; set; }

    }
}
