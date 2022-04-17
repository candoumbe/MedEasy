namespace MedEasy.Wasm.Apis.Identity.v1
{
    public record BearerTokenModel
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }




}
