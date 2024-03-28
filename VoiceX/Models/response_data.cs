using System.Collections.Generic;
using System.Net;

namespace VoiceX.Models
{
    public class response_data : Responce_model
    {
        public List<user_info> data { get; set; }
    }
}
