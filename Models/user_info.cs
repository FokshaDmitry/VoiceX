using System;

namespace VoiceX.Models
{
    public class user_info
    {
        public string? db_id { get; set; }
        public string? client_id { get; set; }
        public string? username { get; set; }
        public DateTime? created_at { get; set; }
        public string? marital_status { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string? phone1 { get; set; }
        public string? email { get; set; }
        public string? lang { get; set; }
        public string? address { get; set; }
        public string? client_id_date { get; set; }
        public string? country { get; set; }
    }
}
