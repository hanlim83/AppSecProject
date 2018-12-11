using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class AWSTestingS3FormModel
    {
        [DisplayName("Bucket Name")]
        [Required]
        public string BucketName { get; set; }
    }
}
