using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class PostingDocument : UploadedFile
    {
        [Display(Name = "Posting")]
        public int PostingID { get; set; }

        public Posting Posting { get; set; }
    }
}
