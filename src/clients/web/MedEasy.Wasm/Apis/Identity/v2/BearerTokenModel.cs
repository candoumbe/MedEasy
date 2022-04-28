namespace MedEasy.Wasm.Apis.Identity.v2
{
    public record BearerTokenModel
    {
        public TokenModel AccessToken { get; set; }

        public TokenModel RefreshToken { get; set; }
    }
}
