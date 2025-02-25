using Newtonsoft.Json;

namespace VoiceX.Models
{
    public class Contact
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("telephone")]
        public string? Telephone { get; set; }
    }
}
