using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
