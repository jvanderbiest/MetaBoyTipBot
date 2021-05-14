using Newtonsoft.Json;

namespace MetaBoyTipBot.Responses
{
    public class ValidateResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("error")]
        public ValidateErrorResponse Error { get; set; }
    }

    public class ValidateErrorResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}