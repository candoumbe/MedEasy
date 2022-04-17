namespace MedEasy.Wasm.Apis.Identity;

using System.ComponentModel.DataAnnotations;

public record LoginModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}
