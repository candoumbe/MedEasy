using System;
using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
