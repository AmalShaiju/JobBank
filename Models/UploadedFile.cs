using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sol_Job_Bank.Models
{
    public class UploadedFile
    {
        public UploadedFile()
        {
            FileContent = new FileContent();
        }
        public int ID { get; set; }

        [StringLength(255)]
        public string MimeType { get; set; }

        [StringLength(255, ErrorMessage = "The name of the file cannot be more than 255 characters.")]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [DataType(DataType.MultilineText)]
        [MinLength(5, ErrorMessage = "The description must be at least 5 characters long.")]
        [MaxLength(2000, ErrorMessage = "The description cannot be more than 2000 characters long")]
        public string Description { get; set; }

        public FileContent FileContent { get; set; }
    }
}
