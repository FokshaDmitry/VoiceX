using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoiceX.Models
{
    public class Get_pauses : Responce_model
    {
        public Status_pause ResponseData { get; set; }
    }
}
