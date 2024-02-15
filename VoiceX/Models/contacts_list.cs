using System.Collections.Generic;
using System.Net;

namespace VoiceX.Models
{
    public class contacts_list
    {
        public HttpStatusCode responseCode { get; set; }
        public string responseMessage { get; set; }
        public List<Contact> contacts { get; set; }
    }
}
