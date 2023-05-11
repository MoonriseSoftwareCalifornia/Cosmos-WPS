using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Author Information
    /// </summary>
    public class AuthorInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "User Id")]
        public string UserId { get; set; }

        [Display(Name = "Author Name")]
        public string AuthorName { get; set; }

        [Display(Name = "About the author")]
        public string AuthorDescription { get; set; }

        [Display(Name = "Twitter Handle")]
        public string TwitterHandle { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = "Instagram Link")]
        public string InstagramUrl { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Public Email")]
        public string EmailAddress { get; set; }
    }
}
