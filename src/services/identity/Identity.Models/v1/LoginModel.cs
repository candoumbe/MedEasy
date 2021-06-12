
namespace Identity.Models.v1
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model for querying an access token
    /// </summary>
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}
