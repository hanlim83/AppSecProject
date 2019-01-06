using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class AWSTestingS3FormModel
    {
        [DisplayName("Bucket Name")]
        [Required]
        public string BucketName { get; set; }
    }
}
