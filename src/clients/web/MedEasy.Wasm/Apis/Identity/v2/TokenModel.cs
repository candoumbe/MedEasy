namespace MedEasy.Wasm.Apis.Identity.v2;

public record TokenModel
{
    public string Token { get; set; }

    public DateTime Expires { get; set; }
}