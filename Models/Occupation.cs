using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Occupation : Auditable
    {
        public Occupation()
        {
            Positions = new HashSet<Position>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave Title blank.")]
        [StringLength(50, ErrorMessage = "Title cannot be more than 50 characters long.")]
        public string Title { get; set; }

        public ICollection<Position> Positions { get; set; }

    }
}
