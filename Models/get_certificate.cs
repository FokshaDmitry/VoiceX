namespace VoiceX.Models
{
    public class Get_certificate : Responce_model
    {
        public string? Error { get; set; }
        public string? P12l { get; set; }
        public string? P12 { get; set; }
        public string? Cert { get; set; }
        public string? Key { get; set; }
        public string? Cn { get; set; }
        public string? App_token { get; set; }
        public Sip_settings? Sip_settings { get; set; }
        public User_data? User_data { get; set; }
    }
}
