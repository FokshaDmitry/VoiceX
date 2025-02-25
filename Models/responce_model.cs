using System.Net;

namespace VoiceX.Models
{
    public class Responce_model
    {
        public HttpStatusCode ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
