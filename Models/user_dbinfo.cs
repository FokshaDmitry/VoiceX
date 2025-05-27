using System.Collections.Generic;

namespace VoiceX.Models
{
    public class user_dbinfo : Responce_model
    {
        public data data {  get; set; }
    }
    public class data : user_info
    {
        public List<object>? statuses { get; set; }
    }
}
