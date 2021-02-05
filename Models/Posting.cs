using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class Posting : Auditable, IValidatableObject
    {
        public Posting()
        {
            Applications = new HashSet<Application>();
            PostingDocuments = new HashSet<PostingDocument>();
        }

        public int ID { get; set; }

        [Display(Name = "Posting")]
        public string PostingSummary
        {
            //Note: If the Position object is null then this just shows 
            //the closing date.
            get
            {
                return (string.IsNullOrEmpty(Position?.Name) ? "" : Position?.Name + " - ") + 
                    "Closing: " + ClosingDate.ToString("yyyy-MM-dd");
            }
        }
        [Display(Name = "Openings")]
        public string OpeningsSummary
        {
            get
            {
                return "(" + NumberOpen.ToString() + (NumberOpen == 1 ? " opening)" : " openings)");
            }
        }
        [Display(Name = "Number of Openings")]
        [Required(ErrorMessage = "You cannot leave Number of Openings blank.")]
        [Range(0,int.MaxValue,ErrorMessage = "Number of Openings cannot be negative")]
        public int NumberOpen { get; set; }

        [Display(Name = "Closing Date")]
        [Required(ErrorMessage = "You cannot leave closing date blank.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ClosingDate { get; set; }

        [Display(Name = "Starting Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Position")]
        [Required(ErrorMessage = "You must select a Position for the Posting")]
        public int PositionID { get; set; }

        public Position Position { get; set; }

        public ICollection<Application> Applications { get; set; }
        public ICollection<PostingDocument> PostingDocuments { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Closing Date in past
            if (ClosingDate < DateTime.Today)
            {
                yield return new ValidationResult("Closing Date cannot be in the past.", new[] { "ClosingDate" });
            }
            //Start Date before Closing Date
            if(StartDate.HasValue)
            {
                if (ClosingDate > StartDate.GetValueOrDefault())
                {
                    yield return new ValidationResult("Starting Date cannot be before the Closing Date.", new[] { "StartDate" });
                }
            }
        }
    }
}
